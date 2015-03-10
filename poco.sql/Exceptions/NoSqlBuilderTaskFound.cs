using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql.Exceptions
{
    public class NoSqlBuilderTaskFound : Exception
    {
        public override string Message
        {
            get
            {
                return "No tasks were found for sql build.";
            }
        }
    }
}
