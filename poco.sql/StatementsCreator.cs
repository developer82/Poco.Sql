using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql
{
    public class StatementsCreator
    {
        public static SqlBuilder Create<T>()
        {
            var dummy = (T) Activator.CreateInstance(typeof (T));
            return new SqlBuilder(dummy);
        }
    }
}
