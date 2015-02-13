using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poco.Sql
{
    public class PocoSqlStoredProceduresMapping
    {
        internal PocoSqlStoredProcedureMap SelectMap { get; private set; }
        internal PocoSqlStoredProcedureMap InsertMap { get; private set; }
        internal PocoSqlStoredProcedureMap UpdateMap { get; private set; }
        internal PocoSqlStoredProcedureMap DeleteMap { get; private set; }

        public PocoSqlStoredProcedureMap Select()
        {
            SelectMap = new PocoSqlStoredProcedureMap();
            return SelectMap;
        }

        public PocoSqlStoredProcedureMap Insert()
        {
            InsertMap = new PocoSqlStoredProcedureMap();
            return InsertMap;
        }

        public PocoSqlStoredProcedureMap Update()
        {
            UpdateMap = new PocoSqlStoredProcedureMap();
            return UpdateMap;
        }

        public PocoSqlStoredProcedureMap Delete()
        {
            DeleteMap = new PocoSqlStoredProcedureMap();
            return DeleteMap;
        }
    }
}
