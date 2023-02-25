using AvaloniaPlannerLib.Data;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProjectPermissions
    {
        public const string TABLE_NAME = "project_permissions";

        [SQLPrimary]
        [SQLSize(36)]
        public string id { get; set; } = "";
        public string project_id { get; set; } = "";
        public string user_id { get; set; } = "";
        public bool can_read { get; set; } = false;
        public bool can_write { get; set; } = false;
        public DateTime issue_date { get; set; } = DateTime.Now;

        public static DbProjectPermissions All(Database db, StringID projectId, StringID userId)
        {
            var id = db.GenerateUniqueIdString(TABLE_NAME, nameof(DbProjectPermissions.id));
            return new DbProjectPermissions()
            {
                id = id,
                project_id = projectId,
                user_id = userId,
                can_read = true,
                can_write = true,
                issue_date = DateTime.Now
            };
        }
    }
}
