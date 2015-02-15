using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using Poco.Sql.Helpers;
using Poco.Sql.Exceptions;

namespace Poco.Sql
{
    /// <summary>
    /// This class is responsible for building the SQL statements
    /// </summary>
    public class QueryBuilder
    {
        private static Dictionary<string, string> _cachedQueries = new Dictionary<string, string>(); //TODO: implement caching
        private static PluralizationService _pluralizationService = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

        private enum QueryType { Select, Insert, Update, Delete, StoredProcedure }

        private object _obj;
        private string
            _sql = String.Empty,
            _graphSql = String.Empty,
            _where = String.Empty;
        private bool _endWithSemicolon = true;
        private QueryType _queryType;

        public QueryBuilder(object obj)
        {
            _obj = obj;
        }

        public QueryBuilder NamedQuery(string name)
        {
            var map = getMapping(_obj);
            if (map != null)
                _sql = map.GetQueryByName(name);
            return this;
        }

        #region Select
        public QueryBuilder Select()
        {
            _queryType = QueryType.Select;
            return buildSqlStatement(_obj, null, null, null, null);
        }

        public QueryBuilder Select(long id)
        {
            _queryType = QueryType.Select;
            string primaryKey = getPrimaryKey(_obj);
            return buildSqlStatement(_obj, null, null, null, null).Where(primaryKey + " = " + id);
        }
        
        public QueryBuilder Select<T>(PocoSqlMapping<T> map)
        {
            _queryType = QueryType.Select;
            return buildSqlStatement(_obj, null, null, null, map);
        }

        public QueryBuilder Select(string tableName)
        {
            _queryType = QueryType.Select;
            return buildSqlStatement(_obj, tableName, null, null, null);
        }

        public QueryBuilder Select(QueryBuilderOptions options)
        {
            _queryType = QueryType.Select;
            return buildSqlStatement(_obj, options.TableName, options.SelectFullGraph, options.PluralizeTableNames, options.Mapping);
        }
        #endregion

        public QueryBuilder FullGraph()
        {
            return this;
            PropertyInfo[] propertyInfos = _obj.GetType().GetProperties();

            StringBuilder allFields = new StringBuilder();
            foreach (PropertyInfo propertyInfo in propertyInfos.Where(p => p.PropertyType.FullName.StartsWith("System.Collections")))
            {
                var collectionObject = propertyInfo.GetValue(_obj);
                Type collectionType = collectionObject.GetType().GetGenericArguments()[0];
                object obj = Activator.CreateInstance(collectionType);

                string key = obj.GetType().FullName;
                if (Configuration.HasMap(key))
                {
                    var map = Configuration.GetMap(key);;
                    var relationship = map.GetRelationship(_obj.GetType().FullName);

                    string currentKey = _obj.GetType().FullName;
                    var currentMap = Configuration.GetMap(key); ;

                    var val = _obj.GetType().GetProperty(currentMap.GetPrimaryKey()).GetValue(_obj);
                    string whereConditionStr = relationship.GetForeignKey() + " = " + val;

                    QueryBuilder qb = new QueryBuilder(obj);
                    string sql = qb.Select().Where(whereConditionStr).ToString();
                    _graphSql += Environment.NewLine + sql;
                }
            }

            return this;
        }

        public QueryBuilder Update()
        {
            _queryType = QueryType.Update;
            buildSqlStatement(_obj, null, null, null, null);
            return this;
        }

        public QueryBuilder Insert()
        {
            _queryType = QueryType.Insert;
            buildSqlStatement(_obj, null, null, null, null);
            return this;
        }

        public QueryBuilder Delete()
        {
            _queryType = QueryType.Delete;
            buildSqlStatement(_obj, null, null, null, null);
            return this;
        }

        private QueryBuilder buildSqlStatement(object obj, string tableName, bool? fullGraph, bool? pluralizeTableNames, IPocoSqlMapping map)
        {
            if (Configuration.Comment)
                _sql = getComment(obj);
            
            if (map == null)
                map = getMapping(obj);

            if (map != null)
            {
                //
                // Custom query
                //
                var customQuery = getCustomQuery(map);
                if (!String.IsNullOrEmpty(customQuery))
                {
                    _sql += customQuery;
                    if (_sql.EndsWith(";")) _endWithSemicolon = false;
                    return this; // if it's a custom query no other thing needs (the user knew in advanced what he wanted) to be done and the result is returned
                }

                //
                // Stored Procedures mappings
                //
                var spMappings = map.GetStoredProceduresMappings();
                if (spMappings != null)
                {
                    PocoSqlStoredProcedureMap spMap = getStoredProcedureMap(spMappings, _queryType);
                    if (spMap != null)
                    {
                        string spName = getStoredProcedureName(spMap, obj, _queryType);
                        _sql += String.Format("{0}{1}{2}",
                            spMap.Execution ? "exec " : String.Empty,
                            spName,
                            spMap.Parameters ?  getQueryFields(map) : String.Empty);
                        if (!spMap.Execution && !spMap.Parameters) _endWithSemicolon = false;
                        return this;
                    }
                }

                if (_queryType != QueryType.Select && map.GetIsVirtual())
                    throw new CantUpdateVirtualException();

                if (String.IsNullOrEmpty(tableName))
                    tableName = map.GetTableName();
            }
            
            if (String.IsNullOrEmpty(tableName))
                tableName = obj.GetType().Name;

            if (map == null || (map != null && String.IsNullOrEmpty(map.GetTableName())))
            {
                if (pluralizeTableNames != false)
                {
                    if (pluralizeTableNames == true || Configuration.IsPluralizeTableNames)
                    {
                        if (_pluralizationService.IsSingular(tableName))
                            tableName = _pluralizationService.Pluralize(tableName);
                    }
                }
            }

            switch (_queryType)
            {
                case QueryType.Select:
                    _sql += String.Format("select {0} from {1}", getQueryFields(map), tableName);
                    break;
                case QueryType.Insert:
                    _sql += String.Format("insert into {0} {1}", tableName, getQueryFields(map));
                    break;
                case QueryType.Update:
                    _sql += String.Format("update {0} set {1}", tableName, getQueryFields(map));
                    break;
                case QueryType.Delete:
                    _sql += String.Format("delete from {0}", tableName);
                    string primaryKey = getPrimaryKey(_obj);
                    string deleteWhereValue = "@" + primaryKey;
                    if (Configuration.ValuesInQueies)
                        deleteWhereValue = getPropertyValueAsSql(_obj, primaryKey);
                    this.Where(String.Format("{0} = {1}", primaryKey, deleteWhereValue));
                    break;
                case QueryType.StoredProcedure:
                    var queryFields = String.Empty;
                    var execCmd = String.Empty;
                    _sql += String.Format("exec {0} {1}", tableName, getQueryFields(map));
                    break;
            }

            return this;
        }

        private PocoSqlStoredProcedureMap getStoredProcedureMap(PocoSqlStoredProceduresMapping spMappings, QueryType queryType)
        {
            switch (queryType)
            {
                case QueryType.Select:
                    return spMappings.SelectMap;
                case QueryType.Insert:
                    return spMappings.InsertMap;
                case QueryType.Update:
                    return spMappings.UpdateMap;
                case QueryType.Delete:
                    return spMappings.DeleteMap;
                default:
                    return null;
            }
        }

        private string getComment(object obj)
        {
            return String.Format(
                "/*{0}Poco.Sql{0}Time: {1}{0}Object: {2}{0}*/{0}",
                Environment.NewLine,
                DateTime.Now,
                obj.GetType().FullName
            );
        }

        private string getCustomQuery(IPocoSqlMapping map)
        {
            string customQuery = null;
            var customMappings = map.GetCustomMappings();
            if (customMappings != null)
            {
                switch (_queryType)
                {
                    case QueryType.Select:
                        customQuery = customMappings.SelectQuery;
                        break;
                    case QueryType.Insert:
                        customQuery = customMappings.InsertQuery;
                        break;
                    case QueryType.Update:
                        customQuery = customMappings.UpdateQuery;
                        break;
                    case QueryType.Delete:
                        customQuery = customMappings.DeleteQuery;
                        break;
                }
            }
            return customQuery;
        }

        private string getQueryFields(IPocoSqlMapping map)
        {
            PropertyInfo[] propertyInfos = _obj.GetType().GetProperties();

            string primaryKey = null;
            
            if (_queryType == QueryType.Update || _queryType == QueryType.Insert)
                primaryKey = getPrimaryKey(_obj);

            StringBuilder allFields = new StringBuilder();
            StringBuilder insertValues = new StringBuilder();
            
            foreach (PropertyInfo propertyInfo in propertyInfos.Where(p => p.PropertyType.FullName.StartsWith("System") && !p.PropertyType.FullName.StartsWith("System.Collections"))) // only loop on objects that are not custom class
            {
                string
                    dbSelect = null,
                    fieldName = null,
                    dbColumnName = null;

                if (map != null)
                {
                    PropertyMap propertyMap = map.GetMapping(propertyInfo.Name);
                    if (propertyMap != null)
                    {
                        if (!propertyMap.Ignored)
                        {
                            if (!propertyMap.ColumnName.Equals(propertyInfo.Name))
                            {
                                if (_queryType == QueryType.Select)
                                    dbSelect = String.Format("{0} as {1}",
                                        propertyMap.ColumnName,
                                        propertyInfo.Name);
                                fieldName = propertyInfo.Name;
                                dbColumnName = propertyMap.ColumnName;
                            }
                        }
                        else
                        {
                            dbSelect = String.Empty;
                        }
                    }
                }

                if (String.IsNullOrEmpty(dbSelect))
                    dbSelect = fieldName = dbColumnName = propertyInfo.Name;

                if (fieldName == primaryKey)
                {
                    if (_queryType == QueryType.Update)
                    {
                        var property = _obj.GetType().GetProperty(dbColumnName);
                        string val = "@" + primaryKey;
                        if (Configuration.ValuesInQueies)
                            property.GetValue(_obj).ToString();
                        this.Where(primaryKey + " = " + val);
                    }
                    
                    if (_queryType == QueryType.Update || (_queryType == QueryType.Insert && map.GetPrimaryAutoGenerated()))
                        continue;
                }
                
                switch(_queryType)
                {
                    case QueryType.Select:
                        allFields.Append((allFields.Length > 0 && !String.IsNullOrEmpty(fieldName) ? ", " : String.Empty) + dbSelect);
                        break;

                    case QueryType.Insert:
                        if (!String.IsNullOrEmpty(fieldName))
                        {
                            string insertVal;
                            if (Configuration.ValuesInQueies)
                                insertVal = getPropertyValueAsSql(_obj, fieldName);
                            else
                                insertVal = "@" + fieldName;

                            allFields.Append((allFields.Length > 0 ? ", " : String.Empty) + dbSelect);
                            insertValues.Append((insertValues.Length > 0 ? ", " : String.Empty) + insertVal);
                        }
                        break;

                    case QueryType.Update:
                    case QueryType.StoredProcedure:
                        string updateVal;
                        if (Configuration.ValuesInQueies)
                            updateVal = getPropertyValueAsSql(_obj, fieldName, true);
                        else
                            updateVal = "@" + fieldName;
                        
                        allFields.Append((allFields.Length > 0 && !String.IsNullOrEmpty(fieldName) ? ", " : String.Empty) + dbColumnName + " = " + updateVal);
                        break;

                    case QueryType.Delete:

                        break;
                }
            }

            if (_queryType == QueryType.Insert)
                return String.Format("({0}) values({1})", allFields, insertValues);
            else
                return allFields.ToString();
        }

        private string getPropertyValueAsSql(object obj, string propertyName)
        {
            return getPropertyValueAsSql(obj, propertyName, false);
        }

        private string getPropertyValueAsSql(object obj, string propertyName, bool cast)
        {
            PropertyInfo property = obj.GetType().GetProperty(propertyName);
            string result = property.GetValue(_obj).ToString();

            if ((property.PropertyType == typeof(string) || !cast) && property.PropertyType != typeof(int))
                result = "'" + result + "'";
            else if (property.PropertyType == typeof(DateTime))
                result = "cast('" + result + "' as datetime)";

            return result;
        }

        private string getStoredProcedureName(PocoSqlStoredProcedureMap spMap, object obj, QueryType queryType)
        {
            if (spMap == null) return String.Empty;
            if (String.IsNullOrEmpty(spMap.Name))
                return String.Format("{0}{1}_{2}", Configuration.StoredProceduresPrefix, obj.GetType().Name, queryType.ToString());

            return spMap.Name;
        }

        private IPocoSqlMapping getMapping(object obj)
        {
            IPocoSqlMapping map = null;
            string key = obj.GetType().FullName;
            if (Configuration.HasMap(key))
                map = Configuration.GetMap(key);
            return map;
        }

        #region Join
        public QueryBuilder Join<T>(Expression<Func<T, bool>> on)
        {
            return Join<T>(typeof(T).Name, on);
        }

        public QueryBuilder Join<T>(string tableName, Expression<Func<T, bool>> on)
        {


            return this;
        }
        #endregion

        #region Where
        public QueryBuilder Where(string condition)
        {
            if (_queryType == QueryType.Update)
                _where = String.Empty;
            _where += " where " + condition;
            return this;
        }

        public QueryBuilder Where<TSource>(Expression<Func<TSource, bool>> condition)
        {
            _where += " where " + ExpressionEvaluator.Eval(condition.Body, Configuration.ValuesInQueies);
            return this;
        }
        #endregion

        public override string ToString()
        {
            return String.Format("{0}{1}{2}{3}",
                _sql,
                _where,
                (_endWithSemicolon ? ";" : String.Empty),
                _graphSql);
        }

        private string getPrimaryKey(object obj)
        {
            var map = getMapping(_obj);
            if (map != null && !String.IsNullOrEmpty(map.GetPrimaryKey()))
                return map.GetPrimaryKey(); // return a mapped primary key

            // check by convension
            PropertyInfo property = null;
            property = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == "id").FirstOrDefault();
            if (property != null)
                return property.Name;

            property = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == obj.GetType().Name.ToLower() + "id").FirstOrDefault();
            if (property != null)
                return property.Name;

            return null;
        }
    }
}
