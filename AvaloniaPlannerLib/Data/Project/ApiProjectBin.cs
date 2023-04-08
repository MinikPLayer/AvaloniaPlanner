using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    public class ApiProjectBin
    {
        public string Id { get; set; } = "";
        public string Project_id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Archived { get; set; } = false;
        public int Position { get; set; } = 0;
        public int ShownTaskCount { get; set; } = -1;
        public bool DefaultStatusEnabled { get; set; } = false;
        public ProjectStatus DefaultStatus { get; set; } = ProjectStatus.Unknown;
        
        public List<ApiProjectTask> Tasks { get; set; } = new List<ApiProjectTask>();
    }
}
