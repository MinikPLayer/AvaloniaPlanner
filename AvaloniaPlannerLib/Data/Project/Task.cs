using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    [SqlTable("ProjectTasks")]
    public class Task
    {
        [SQLPrimary]
        public long Id { get; set; } = -1;
        public long Project_id { get; set; } = -1;
        public long Bin_id { get; set; } = -1;
        public string Name { get; set; } = "";
        public Status Status { get; set; } = Status.Undefinied;
    }
}
