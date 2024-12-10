using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NgLoader.Queries
{
    public class QuerySpecification<TEntity, TDto>
        where TEntity : class
        where TDto : class, new()
    {
        #region Fields

        private readonly IReadOnlyDictionary<string, LambdaExpression> dtoMappings;
        private readonly Expression<Func<TEntity, TDto>> constructorExpression;

        #endregion

        #region Constructor

        public QuerySpecification(IReadOnlyDictionary<string, LambdaExpression> dtoMapping, Expression<Func<TEntity, TDto>> constructorExpression)
        {
            this.dtoMappings = dtoMapping;
            this.constructorExpression = constructorExpression;
        }

        #endregion

        #region Properties

        public Expression<Func<TEntity, TDto>> ConstructorExperssion => constructorExpression;

        public IReadOnlyDictionary<string, LambdaExpression> DtoMappings => dtoMappings;

        #endregion
    }
}
