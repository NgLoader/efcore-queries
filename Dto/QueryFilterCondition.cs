﻿using System.Text.Json.Serialization;
using NgLoader.Queries.Converters;

namespace NgLoader.Queries.Dto
{
    public class QueryFilterCondition : IQueryFilter
    {
        public QueryFilterType FilterType => QueryFilterType.Condition;

        public string Key { get; set; } = null!;

        [JsonConverter(typeof(QueryFilterValueConverter))]
        public object Value { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryFilterConditionOperator Operator { get; set; }

        public QueryFilterOptions Options { get; set; } = QueryFilterOptions.None;
    }
}
