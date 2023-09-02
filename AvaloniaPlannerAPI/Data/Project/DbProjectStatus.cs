using AvaloniaPlannerLib.Data;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProjectStatus
    {
        public const string TABLE_NAME = "project_status";

        [SQLPrimary]
        [SQLSize(36)]
        public string id { get; set; } = "";
        public string project_id { get; set; } = "";
        public ProjectStatus status { get; set; } = ProjectStatus.Unknown;
        public DateTime date { get; set; } = DateTime.Now;

        public DbProjectStatus() { }
        public DbProjectStatus(Database db, StringID projectId, ProjectStatus status)
        {
            this.id = db.GenerateUniqueIdString(TABLE_NAME, nameof(DbProjectStatus.id));
            this.project_id = projectId;
            this.status = status;
            this.date = DateTime.Now;
        }
        
    }
}
