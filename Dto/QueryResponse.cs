using System.Collections.Generic;

namespace NgLoader.Queries.Dto
{
    public class QueryResponse<TDto>
    {
        public ICollection<TDto> Results { get; set; } = null!;

        public QueryState State { get; set; } = null!;
    }
}
