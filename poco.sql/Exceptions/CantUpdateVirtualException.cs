using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql.Exceptions
{
    public class CantUpdateVirtualException : Exception
    {
        public override string Message
        {
            get
            {
                return "You are trying to create a SQL statement for virtual/view mapping object, which does not represet an actual table in the database";
            }
        }
    }
}
