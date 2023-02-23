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
        public long Id { get; set; } = -1;
        public long Project_id { get; set; } = -1;
        public long Bin_id { get; set; } = -1;
        public string Name { get; set; } = "";
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
    }
}
