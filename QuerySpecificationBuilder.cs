using Imprex.Queries.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Imprex.Queries
{
    public class QuerySpecificationBuilder<TEntity, TDto>
        where TEntity : class
        where TDto : class, new()
    {
        #region Constants

        private static readonly MethodInfo SelectMethod = Queryable.AsQueryable(typeof(Enumerable).GetMethods())
            .Where(e => e.Name == "Select")
            .Where(e => e.GetParameters().Length == 2)
            .Where(e => e.GetParameters()[1].ParameterType.GenericTypeArguments.Length == 2)
            .First()
            ?? throw new Exception("IEnumerable::Select(entity, dto) is missing.");

        #endregion

        #region Fields

        private readonly Dictionary<PropertyInfo, LambdaExpression> dtoMapping = new();

        #endregion

        #region Constructors
        public QuerySpecificationBuilder() { }

        #endregion

        #region Registers

        public QuerySpecificationBuilder<TEntity, TDto> Register<TEntityParameter, TDtoParameter>(
            Expression<Func<TEntity, IEnumerable<TEntityParameter>>> entityExpression,
            Expression<Func<TEntityParameter, TDtoParameter>> transformationExpression,
            Expression<Func<TDto, IEnumerable<TDtoParameter>>> dtoExpression)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity));
            Expression entityBody = ExpressionModifier.BindLambdaBody(entityExpression, entityParameter);

            var transformationParameter = Expression.Parameter(typeof(TEntityParameter));
            var transformationBody = ExpressionModifier.BindLambdaBody(transformationExpression, transformationParameter);
            var transformationLambda = Expression.Lambda<Func<TEntityParameter, TDtoParameter>>(transformationBody, transformationParameter);

            var selectMethod = SelectMethod.MakeGenericMethod(typeof(TEntityParameter), typeof(TDtoParameter));
            var selectCall = Expression.Call(
                selectMethod,
                entityBody,
                transformationLambda);

            var lambda = Expression.Lambda<Func<TEntity, IEnumerable<TDtoParameter>>>(
                Expression.Condition(
                    Expression.Equal(entityBody, Expression.Constant(null)),
                    Expression.Constant(null, typeof(IEnumerable<TDtoParameter>)),
                    selectCall
                ), entityParameter);

            return Register(lambda, dtoExpression);
        }

        public QuerySpecificationBuilder<TEntity, TDto> Register<TEntityParameter, TDtoParameter>(
            Expression<Func<TEntity, TEntityParameter>> entityExpression,
            Expression<Func<TEntityParameter, TDtoParameter>> tranformationExpression,
            Expression<Func<TDto, TDtoParameter>> dtoExpression)
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            
            Expression entity = ExpressionModifier.BindLambdaBody(entityExpression, parameter);
            Expression transformation = ExpressionModifier.BindLambdaBody(tranformationExpression, entity);

            var lambda = Expression.Lambda<Func<TEntity, TDtoParameter>>(
                Expression.Condition(
                    Expression.Equal(entity, Expression.Constant(null)),
                    Expression.Constant(null, typeof(TDtoParameter)),
                    transformation
                ), parameter);

            return Register(lambda, dtoExpression);
        }

        public QuerySpecificationBuilder<TEntity, TDto> Register<TParameter>(Expression<Func<TEntity, TParameter>> entityExpression, Expression<Func<TDto, TParameter>> dtoExpression)
        {
            if (dtoExpression.Body is not MemberExpression member ||
                member.Member is not PropertyInfo propertyInfo ||
                !propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                throw new ArgumentException($"Expression {dtoExpression} is not a MemberExpression of a read- or writeable property.");
            }

            dtoMapping[propertyInfo] = entityExpression;
            return this;
        }

        public QuerySpecificationBuilder<TEntity, TDto> Register<TParameter>(Expression<Func<TEntity, IEnumerable<TParameter>>> entityExpression, Expression<Func<TDto, IEnumerable<TParameter>>> dtoExpression)
        {
            if (dtoExpression.Body is not MemberExpression member ||
                member.Member is not PropertyInfo propertyInfo ||
                !propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                throw new ArgumentException($"Expression {dtoExpression} is not a MemberExpression of a read- or writeable property.");
            }

            dtoMapping[propertyInfo] = entityExpression;
            return this;
        }

        #endregion

        #region Builders

        public Expression<Func<TEntity, TDto>> CreateConstructorExpression()
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            return Expression.Lambda<Func<TEntity, TDto>>(
                Expression.MemberInit(
                    Expression.New(typeof(TDto)),
                    dtoMapping.Select(e => Expression.Bind(e.Key, ExpressionModifier.BindLambdaBody(e.Value, parameter)))),
                parameter);
        }

        public QuerySpecification<TEntity, TDto> Build()
        {
            return new QuerySpecification<TEntity, TDto>(
                dtoMapping.ToDictionary(e => e.Key.Name, e => e.Value).AsReadOnly(),
                CreateConstructorExpression());
        }

        #endregion
    }
}
