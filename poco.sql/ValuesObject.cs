using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Poco.Sql
{
    public class ValuesObject
    {
        public static object Create(params object[] args)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            List<string> sqlParams = null;
            int startPos = 0;

            if (args.Length > 1 && args[0] is String)
            {
                startPos = 1;
                sqlParams = new List<string>();
                
                string sql = args[0].ToString();
                Regex regex = new Regex(@"(?<=@)([\w\-]+)");
                var matches = regex.Matches(sql);

                for (int i = 0; i < matches.Count; i++)
                {
                    var val = matches[i].Value;
                    if (!sqlParams.Contains(val))
                        sqlParams.Add(matches[i].Value);
                }
            }

            for (int i = startPos; i < args.Length; i++)
            {
                object currentObj = args[i];
                if (currentObj == null) continue;
                PropertyInfo[] propertyInfos = currentObj.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfos.Where(p => p.PropertyType.FullName.StartsWith("System") && !p.PropertyType.FullName.StartsWith("System.Collections"))) // only loop on objects that are not custom class
                {
                    if (values.ContainsKey(propertyInfo.Name)) continue;
                    var property = currentObj.GetType().GetProperty(propertyInfo.Name);
                    values.Add(propertyInfo.Name, property.GetValue(currentObj));
                }
            }

            object obj = createDummyObject(values, sqlParams);
            return obj;
        }

        private static object createDummyObject(Dictionary<string, object> values, List<string> sqlParams)
        {
            // Code for creating .NET objects at runtime thanks to:
            // http://benohead.com/create-anonymous-types-at-runtime-in-c-sharp/

            AssemblyBuilder dynamicAssembly =
                AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Poco.Sql.Assembly"),
                AssemblyBuilderAccess.Run);

            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("Poco.Sql.Module");
            TypeBuilder dynamicType = dynamicModule.DefineType("Poco.Sql.DynamicType", TypeAttributes.Public);
            
            foreach(string key in values.Keys)
            {
                if (sqlParams == null || sqlParams.Contains(key))
                {
                    object val = values[key];
                    addProperty(dynamicType, key, val.GetType());
                }
            }
            
            Type myType = dynamicType.CreateType();
            object obj = Activator.CreateInstance(myType);

            foreach (string key in values.Keys)
            {
                if (sqlParams == null || sqlParams.Contains(key))
                    myType.GetProperty(key).SetValue(obj, values[key]);
            }

            return obj;
        }

        private static void addProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

            FieldBuilder field = typeBuilder.DefineField("_" + propertyName, typeof(string), FieldAttributes.Private);
            PropertyBuilder property = typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.None, propertyType, new[] { propertyType });

            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_value", getSetAttr, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_value", getSetAttr, null, new[] { propertyType });
            ILGenerator setIl = setMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            property.SetGetMethod(getMethodBuilder);
            property.SetSetMethod(setMethodBuilder);
        }
    }
}
