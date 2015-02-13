using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql
{
    public class QueryBuilderOptions
    {
        public string TableName { get; set; }
        public bool? PluralizeTableNames { get; set; }
        public bool? SelectFullGraph { get; set; }
        public IPocoSqlMapping Mapping { get; set; }
    }
}
