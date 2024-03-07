using System;

namespace Imprex.Queries.Exceptions
{
    public class QueryLimitException : Exception
    {
        public QueryLimitException(string message) : base(message) { }
    }
}
