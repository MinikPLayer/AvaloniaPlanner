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
    }
}
