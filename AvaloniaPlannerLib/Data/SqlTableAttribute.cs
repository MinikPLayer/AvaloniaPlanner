using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data
{
    public class SqlTableAttribute : Attribute
    {
        public string TableName { get; set; } = "";
        public SqlTableAttribute(string name)
        {
            this.TableName = name;
        }

        public static void CreateStructure(Database DB)
        {
            if (DB is null)
                throw new NullReferenceException("Not initialized");

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(SqlTableAttribute)));
            var tables = new List<(string table, Type type)>();
            tables.AddRange(types.Select(x =>
            {
                var attr = x.GetCustomAttribute<SqlTableAttribute>();
                if (attr == null)
                    return ("", x);

                return (attr.TableName, x);
            }).Where(x => !string.IsNullOrEmpty(x.Item1)));

            DB.CreateDBStruct(tables);
        }
    }
}
