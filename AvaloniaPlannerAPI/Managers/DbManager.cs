using AvaloniaPlannerLib.Data;
using CSUtil.DB;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace AvaloniaPlannerAPI.Managers
{
    public static class DbManager
    {
        public static Database? DB = null;

        public static void Initialize(string dbString)
        {
            DB = new Database();
            DB.Connect(dbString);

            SqlTableAttribute.CreateStructure(DB);
        }
    }
}
