using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprex.Queries.Options
{
    public class QueryOptions
    {
        public QueryLimits Limits { get; init; } = null!;

        public int DefaultPageSize { get; init; }
    }
}
