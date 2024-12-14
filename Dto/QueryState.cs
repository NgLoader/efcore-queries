using System.Collections.Generic;

namespace NgLoader.Queries.Dto
{
    public class QueryState
    {
        public IQueryFilter? Filter { get; set; }

        public ICollection<QuerySort>? Sorts { get; set; }

        public QueryPagination? Pagination { get; set; }
    }
}
