using System;

namespace NgLoader.Queries.Exceptions
{
    public class QueryLimitException : Exception
    {
        public QueryLimitException(string message) : base(message) { }
    }
}
