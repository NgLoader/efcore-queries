using System.Text.Json.Serialization;

namespace NgLoader.Queries.Dto
{
    public class QuerySort
    {
        public string Key { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QuerySortDirection Direction;
    }
}
