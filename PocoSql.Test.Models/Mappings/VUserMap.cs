using Poco.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoSql.Test.Models.Mappings
{
    public class VUserMap : PocoSqlMapping<VUser>
    {
        public VUserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserId);

            this.IsVirtual();
        }
    }
}
