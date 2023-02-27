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
        [SQLSize(36)]
        public string Id { get; set; } = "";
        public string Project_id { get; set; } = "";
        public string Bin_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Priority { get; set; } = 0;

        public DbProjectTask() { }
        public DbProjectTask(Database db, string projectId, string binId, string name, string description, int priority)
        {
            this.Id = db.GenerateUniqueIdString(TABLE_NAME, nameof(DbProjectTask.Id));
            this.Project_id = projectId;
            this.Bin_id = binId;
            this.Name = name;
            this.Description = description;
            this.Priority = priority;
        }
    }
}
