using AvaloniaPlannerLib.Data;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data
{
    [SqlTable(TABLE_NAME)]
    public class DbUser
    {
        public const string TABLE_NAME = "users";

        [SQLPrimary]
        [SQLSize(36)]
        public string Id { get; set; } = "";
        public string Login { get; set; } = "";
        public string Username { get; set; } = "";
        [SQLSize(64)]
        public byte[] Password { get; set; } = new byte[36];
        [SQLSize(64)]
        public byte[] Salt { get; set; } = new byte[36];
        public string Role { get; set; } = "";
    }
}
