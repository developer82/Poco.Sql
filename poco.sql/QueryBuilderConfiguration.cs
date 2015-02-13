using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql
{
    public class QueryBuilderConfiguration
    {
        public bool PluralizeTableNames { get; set; }
        public bool SelectFullGraphAsDefault { get; set; }
        public bool InjectValuesToQueies { get; set; }
        public bool CachingDisabled { get; set; }
        public bool Comment { get; set; }
        public string StoredProceduresPrefix { get; set; }
    }
}
