using NgLoader.Queries.Dto;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NgLoader.Queries.Extensions
{
    public static class IQueryableExtension
    {
        
        public static async Task<QueryResponse<TDto>> Query<TEntity, TDto>(this IQueryable<TEntity> query, QueryBuilder<TEntity, TDto> queryBuilder, QueryState state, CancellationToken cancel = default)
            where TEntity : class
            where TDto : class, new()
        {
            query = queryBuilder.Filter(query, state.Filter);
            query = queryBuilder.Sort(query, state.Sorts);
            query = await queryBuilder.Pagination(query, state, cancel);

            return await queryBuilder.Response(query, state, cancel);
        }
    }
}
