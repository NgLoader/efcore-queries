using Imprex.Queries.Dto;
using Imprex.Queries.Options;
using Imprex.Queries.Services;
using Imprex.Queries.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Streamevent.Data.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Imprex.Queries
{
    public class QueryBuilder<TEntity, TDto>
        where TEntity : class
        where TDto : class, new()
    {
        #region Constants

        private static readonly MethodInfo CompareMethod = typeof(string).GetMethod("Compare", [typeof(string), typeof(string)])
            ?? throw new Exception("string::Compare(string, StringComparison) is missing.");

        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", [typeof(string)])
            ?? throw new Exception("string::Contains(string, StringComparison) is missing.");

        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", [typeof(string)])
            ?? throw new Exception("string::StartsWith(string, StringComparison) is missing.");

        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", [typeof(string)])
            ?? throw new Exception("string::EndsWith(string, StringComparison) is missing.");

        private static readonly MethodInfo CollateMethod = Queryable.AsQueryable(typeof(RelationalDbFunctionsExtensions).GetMethods())
            .Where(e => e.Name == nameof(RelationalDbFunctionsExtensions.Collate))
            .Where(e => e.GetParameters().Length == 3)
            .Where(e => e.GetGenericArguments().Length == 1)
            .First()
            ?? throw new Exception("string::EndsWith(string, StringComparison) is missing.");

        private static readonly MethodInfo OrderByMethod = GetOrderMethod(typeof(Queryable), typeof(IQueryable<object>), "OrderBy");
        private static readonly MethodInfo OrderByDescendingMethod = GetOrderMethod(typeof(Queryable), typeof(IQueryable<object>), "OrderByDescending");
        private static readonly MethodInfo ThenByMethod = GetOrderMethod(typeof(Queryable), typeof(IOrderedQueryable<object>), "ThenBy");
        private static readonly MethodInfo ThenByDescendingMethod = GetOrderMethod(typeof(Queryable), typeof(IOrderedQueryable<object>), "ThenByDescending");

        private static MethodInfo GetOrderMethod(Type source, Type genericSource, string name)
        {
            return source.GetMethods()
                .Where(m => m.Name == name)
                .Where(m => m.GetParameters().Length == 2)
                .Where(m => m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == genericSource.GetGenericTypeDefinition())
                .Where(m => typeof(LambdaExpression).IsAssignableFrom(m.GetParameters()[1].ParameterType))
                .Where(m => m.GetGenericArguments().Length == 2)
                .FirstOrDefault() ?? throw new Exception($"Can't get order method {name} from {source.FullName}.");
        }
        #endregion

        #region Fields

        private readonly ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));

        private readonly QueryService service;
        private readonly QuerySpecification<TEntity, TDto> specification;

        #endregion

        #region Constructor

        internal QueryBuilder(QueryService service, QuerySpecification<TEntity, TDto> specification)
        {
            this.service = service;
            this.specification = specification;
        }

        #endregion

        #region Properties

        private QueryOptions Options => service.Options;

        private QueryLimits Limits => service.Options.Limits;

        private IReadOnlyDictionary<string, LambdaExpression> DtoMappings => specification.DtoMappings;

        #endregion

        #region Filter

        public Expression<Func<TEntity, bool>> Filter(IQueryFilter? filter)
        {
            Expression? conditions = CreateExpressionTree(filter, 0);
            if (conditions == null)
            {
                return e => true;
            }

            return Expression.Lambda<Func<TEntity, bool>>(
                conditions,
                parameterExpression);
        }
        public IQueryable<TEntity> Filter(IQueryable<TEntity> entities, IQueryFilter? filter)
        {
            return entities.Where(Filter(filter));
        }

        private Expression? CreateExpressionTree(IQueryFilter? queryFilter, int depth)
        {
            if (queryFilter == null)
            {
                return null;
            }

            if (depth > Limits.FilterCompositionDepth)
            {
                throw new QueryLimitException($"The maximum depth allowed is {Limits.FilterCompositionDepth}.");
            }

            if (queryFilter is QueryFilterCondition condition)
            {
                return CreateConditionExpression(condition);
            }
            else if (queryFilter is QueryFilterComposition composition)
            {
                int conditionCount = composition.Conditions.Count;
                if (conditionCount == 0)
                {
                    return null;
                }
                else if (conditionCount > Limits.FilterConditionDepth)
                {
                    throw new QueryLimitException($"A maximum amount of {Limits.FilterConditionDepth} is only allowed for conditions.");
                }

                Expression? expression = null;
                foreach (IQueryFilter entry in composition.Conditions)
                {
                    Expression? entryResult = CreateExpressionTree(entry, depth + 1);
                    if (entryResult == null)
                    {
                        continue;
                    }
                    else if (expression == null)
                    {
                        expression = entryResult;
                        continue;
                    }

                    expression = composition.Operator switch
                    {
                        QueryFilterCompositionOperator.Or => Expression.Or(entryResult, expression),
                        QueryFilterCompositionOperator.And => Expression.And(entryResult, expression),
                        _ => throw new Exception($"Unknown FilterCompositionOperator type: {nameof(composition.Operator)}")
                    };
                }

                return expression;
            }

            throw new Exception($"Unknown IQueryFilter type: {nameof(queryFilter)}");
        }

        private Expression CreateConditionExpression(QueryFilterCondition condition)
        {
            if (!DtoMappings.TryGetValue(condition.Key, out var entityExpression))
            {
                throw new Exception($"Unknown filter key: {condition.Key}");
            }

            Expression entityValue = ExpressionModifier.BindLambdaBody(entityExpression, parameterExpression);
            Expression filterValue = Expression.Constant(condition.Value);

            if (condition.Value is string)
            {
                entityValue = condition.Options.HasFlag(QueryFilterOptions.IgnoreCase)
                    ? entityValue
                    : Expression.Call(CollateMethod.MakeGenericMethod(entityValue.Type), Expression.Constant(EF.Functions), entityValue, Expression.Constant("utf8mb4_bin"));

                return condition.Operator switch
                {
                    QueryFilterConditionOperator.Equals => Expression.Equal(entityValue, filterValue),
                    QueryFilterConditionOperator.NotEquals => Expression.NotEqual(entityValue, filterValue),
                    QueryFilterConditionOperator.GreaterThan => Expression.GreaterThan(
                        Expression.Call(CompareMethod, entityValue, filterValue),
                        Expression.Constant(0)),
                    QueryFilterConditionOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(
                        Expression.Call(CompareMethod, entityValue, filterValue),
                        Expression.Constant(0)),
                    QueryFilterConditionOperator.LessThan => Expression.LessThan(
                        Expression.Call(CompareMethod, entityValue, filterValue),
                        Expression.Constant(0)),
                    QueryFilterConditionOperator.LessThanOrEqual => Expression.LessThanOrEqual(
                        Expression.Call(CompareMethod, entityValue, filterValue),
                        Expression.Constant(0)),
                    QueryFilterConditionOperator.Contains => Expression.Call(entityValue, ContainsMethod, filterValue),
                    QueryFilterConditionOperator.NotContains => Expression.Not(Expression.Call(entityValue, ContainsMethod, filterValue)),
                    QueryFilterConditionOperator.StartsWith => Expression.Call(entityValue, StartsWithMethod, filterValue),
                    QueryFilterConditionOperator.EndsWith => Expression.Call(entityValue, EndsWithMethod, filterValue),
                    _ => throw new Exception("Operator is not supported on string type.")
                };
            }

            // == or != is only supported for type boolean
            if (condition.Value is bool && !(condition.Operator is QueryFilterConditionOperator.Equals or QueryFilterConditionOperator.NotEquals))
            {
                throw new Exception("Operator is not supported on boolean type.");
            }

            return condition.Operator switch
            {
                QueryFilterConditionOperator.Equals => Expression.Equal(entityValue, filterValue),
                QueryFilterConditionOperator.NotEquals => Expression.NotEqual(entityValue, filterValue),
                QueryFilterConditionOperator.GreaterThan => Expression.GreaterThan(entityValue, filterValue),
                QueryFilterConditionOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(entityValue, filterValue),
                QueryFilterConditionOperator.LessThan => Expression.LessThan(entityValue, filterValue),
                QueryFilterConditionOperator.LessThanOrEqual => Expression.LessThanOrEqual(entityValue, filterValue),
                _ => throw new Exception("Operator is not supported on non string types.")
            };
        }

        #endregion

        #region Sort

        public IQueryable<TEntity> Sort(IQueryable<TEntity> entities, ICollection<QuerySort>? sorts)
        {
            if (sorts == null)
            {
                return entities;
            }

            if (sorts.Count > Limits.SortLimit)
            {
                throw new QueryLimitException($"The maximum allowed sort size is {Limits.SortLimit}.");
            }

            bool first = true;
            foreach (QuerySort sort in sorts)
            {
                if (!DtoMappings.TryGetValue(sort.Key, out var entityExpression))
                {
                    throw new Exception($"Unknown filter key: {sort.Key}");
                }

                MethodInfo methodInfo = first
                    ? sort.Direction == QuerySortDirection.Descending
                        ? OrderByDescendingMethod
                        : OrderByMethod
                    : sort.Direction == QuerySortDirection.Descending
                        ? ThenByDescendingMethod
                        : ThenByMethod;
                first = false;

                entities = (IOrderedQueryable<TEntity>)methodInfo
                    .MakeGenericMethod(typeof(TEntity), entityExpression.ReturnType)
                    .Invoke(null, [entities, entityExpression])!;
            }

            return entities;
        }

        #endregion

        #region Pagination

        public async Task<IQueryable<TEntity>> Pagination(IQueryable<TEntity> entities, QueryState state, CancellationToken cancel)
        {
            QueryPagination pagination = state.Pagination ??= new QueryPagination();

            // check if total count already known so we don't need to check it again
            if (pagination.TotalCount == 0)
            {
                pagination.TotalCount = await entities.CountAsync(cancel);
            }

            // get properties or default initalized with 0
            int pageSize = pagination.PageSize;
            int pageIndex = pagination.PageIndex;

            // check if pageSize is higher than zero or else use the default value
            if (pageSize < 1)
            {
                pageSize = Options.DefaultPageSize;

                // update pageSize inside the response
                pagination.PageSize = pageSize;
            }
            // check if pageSize reached the pageSizeLimit
            else if (pageSize > Limits.PageSizeLimit)
            {
                throw new QueryLimitException($"The maximum PageSize is {Limits.PageSizeLimit}.");
            }

            // validate that the pageIndex is not negative
            if (pageIndex < 0)
            {
                throw new QueryLimitException("PageIndex is negative.");
            }

            // return query with pagination result
            return entities
                .Skip(pageSize * pageIndex)
                .Take(pageSize);
        }

        #endregion

        #region Response

        public async Task<QueryResponse<TDto>> Response(IQueryable<TEntity> entities, QueryState state, CancellationToken cancel)
        {
            QueryResponse<TDto> response = new QueryResponse<TDto>();

            response.State = state;
            response.Results = await entities
                .Select(specification.ConstructorExperssion)
                .ToListAsync(cancel);

            return response;
        }

        #endregion
    }
}
