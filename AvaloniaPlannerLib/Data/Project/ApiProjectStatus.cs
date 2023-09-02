using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    public class ApiProjectStatus
    {
        public string id { get; set; } = "";
        public string project_id { get; set; } = "";
        public ProjectStatus status { get; set; } = ProjectStatus.Unknown;
        public DateTime date { get; set; } = DateTime.Now;
    }
}
