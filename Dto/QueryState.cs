using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NgLoader.Queries.Dto
{
    public class QueryState
    {
        public IQueryFilter? Filter { get; set; }

        public ICollection<QuerySort>? Sorts { get; set; }

        public QueryPagination? Pagination { get; set; }
    }
}
