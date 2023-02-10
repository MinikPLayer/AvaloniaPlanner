using AvaloniaPlannerLib.Data;
using CSUtil.DB;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace AvaloniaPlannerAPI.Managers
{
    public static class DbManager
    {
        public static Database? DB = null;

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

        public static void Initialize(string dbString)
        {
            DB = new Database();
            DB.Connect(dbString);

            CreateStructure(DB);
        }
    }
}
