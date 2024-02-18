namespace Imprex.Queries.Options
{
    public class QueryLimits
    {
        public int FilterCompositionDepth { get; init; }

        public int FilterConditionDepth { get; init; }

        public int SortLimit {  get; init; }

        public int PageSizeLimit { get; init; }
    }
}
