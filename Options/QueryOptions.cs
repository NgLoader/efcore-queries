namespace NgLoader.Queries.Options
{
    public class QueryOptions
    {
        public QueryLimits Limits { get; init; } = null!;

        public int DefaultPageSize { get; init; }
    }
}
