using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    public class ApiProjectTask
    {
        public string Id { get; set; } = "";
        public string Project_id { get; set; } = "";
        public string Bin_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Priority { get; set; } = 0;
        public DateTime Deadline { get; set; } = DateTime.MaxValue;
        public ProjectStatus status { get; set; } = ProjectStatus.Unknown;
    }
}
