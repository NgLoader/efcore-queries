using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NgLoader.Queries.Dto
{
    public class QueryFilterComposition : IQueryFilter
    {
        public QueryFilterType FilterType => QueryFilterType.Composition;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryFilterCompositionOperator Operator { get; set; }

        public ICollection<IQueryFilter> Conditions { get; set; } = null!;
    }
}
