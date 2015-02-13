using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql.Exceptions
{
    public class FeatureRequiresMappingException : Exception
    {
        public override string Message
        {
            get
            {
                return this.Source + ": This feature requires a mapping configuration for object. ";
            }
        }
    }
}
