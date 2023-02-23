using AvaloniaPlannerLib.Data;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProjectPermissions
    {
        public const string TABLE_NAME = "project_permissions";

        [SQLPrimary]
        public long id { get; set; } = -1;
        public long project_id { get; set; } = -1;
        public long user_id { get; set; } = -1;
        public bool can_read { get; set; } = false;
        public bool can_write { get; set; } = false;
        public DateTime issue_date { get; set; } = DateTime.Now;

        public static DbProjectPermissions All(Database db, long projectId, long userId)
        {
            var id = db.GenerateUniqueIdLong(TABLE_NAME, nameof(DbProjectPermissions.id));
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
