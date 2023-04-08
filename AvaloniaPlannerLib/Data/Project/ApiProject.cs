using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    
    public class ApiProject
    {
        public static readonly List<ProjectStatus> DisabledStatuses = new()
        {
            ProjectStatus.Completed,
            ProjectStatus.Abandoned,
            ProjectStatus.Archived
        };
        
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Owner { get; set; } = "";

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        public bool DeadlineEnabled { get; set; } = false;
        public DateTime Deadline { get; set; } = DateTime.Now;
        public bool IsDeadlineApplicable => !DisabledStatuses.Contains(Status) && DeadlineEnabled; 

        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
        public List<ApiProjectBin> Bins { get; set; } = new List<ApiProjectBin>();
    }
}
