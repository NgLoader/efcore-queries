namespace NgLoader.Queries.Dto
{
    public enum QueryFilterConditionOperator
    {
        Equals = 0,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,

        Contains,
        NotContains,
        StartsWith,
        EndsWith,

        Includes,
        NotIncludes
    }
}
