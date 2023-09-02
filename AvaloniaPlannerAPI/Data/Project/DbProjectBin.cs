using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProjectBin
    {
        public const string TABLE_NAME = "project_bins";

        [SQLPrimary]
        [SQLSize(36)]
        public string Id { get; set; } = "";
        public string Project_id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Archived { get; set; } = false;
        public int Position { get; set; } = 0;
    }
}
