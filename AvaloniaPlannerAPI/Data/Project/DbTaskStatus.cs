using AvaloniaPlannerLib.Data;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbTaskStatus
    {
        public const string TABLE_NAME = "task_status";

        [SQLPrimary]
        [SQLSize(36)]
        public string id { get; set; } = "";
        public string task_id { get; set; } = "";
        public ProjectStatus status { get; set; } = ProjectStatus.Unknown;
        public DateTime date { get; set; } = DateTime.Now;

        public DbTaskStatus() { }
        public DbTaskStatus(Database db, string taskId, ProjectStatus status)
        {
            this.id = db.GenerateUniqueIdString(TABLE_NAME, nameof(id));
            this.task_id = taskId;
            this.status = status;
            this.date = DateTime.Now;
        }
    }
}
