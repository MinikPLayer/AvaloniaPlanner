using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProjectTask
    {
        public const string TABLE_NAME = "project_tasks";

        [SQLPrimary]
        public string Id { get; set; } = "";
        public string Project_id { get; set; } = "";
        public string Bin_id { get; set; } = "";
        public string Name { get; set; } = "";
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
    }
}
