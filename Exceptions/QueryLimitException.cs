using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamevent.Identity.Exceptions
{
    public class QueryLimitException : Exception
    {
        public QueryLimitException(string message) : base(message) { }
    }
}
