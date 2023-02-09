using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    [SqlTable("Projects")]
    public class Project
    {
        [SQLPrimary]
        public long Id { get; set; } = -1;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Author { get; set; } = "";

        public DateTime CreationDate { get; set; } = DateTime.MinValue;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;

        public Status Status { get; set; } = Status.Undefinied;
    }
}
