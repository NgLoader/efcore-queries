using Imprex.Queries.Options;
using Microsoft.Extensions.Options;

namespace Imprex.Queries.Services
{
    public class QueryService
    {
        #region Fields

        private readonly QueryOptions options;

        #endregion

        #region Constructor

        public QueryService(IOptions<QueryOptions> options)
        {
            this.options = options.Value;
        }

        #endregion

        #region Properties

        internal QueryOptions Options => options;

        #endregion

        #region Builder Creation

        public QueryBuilder<TEntity, TDto> For<TEntity, TDto>(QuerySpecification<TEntity, TDto> specification)
            where TEntity : class
            where TDto : class, new()
        {
            return new QueryBuilder<TEntity, TDto>(this, specification);
        }

        #endregion
    }
}
