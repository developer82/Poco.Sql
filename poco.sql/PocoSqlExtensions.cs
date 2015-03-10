using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql.Extensions
{
    public static class PocoSqlExtensions
    {
        public static SqlBuilder PocoSql(this Object obj)
        {
            return new SqlBuilder(obj);
        }
    }
}
