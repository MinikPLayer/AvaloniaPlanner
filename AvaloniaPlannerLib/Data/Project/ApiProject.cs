using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    public class ApiProject
    {
        public long Id { get; set; } = -1;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public long Owner { get; set; } = -1;

        public DateTime CreationDate { get; set; } = DateTime.MinValue;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;

        public ProjectStatus Status { get; set; } = ProjectStatus.Undefinied;
    }
}
