using AvaloniaPlannerLib.Data;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data
{
    [SqlTable("users")]
    public class DbUser
    {
        [SQLPrimary]
        public long Id { get; set; } = -1;
        public string Login { get; set; } = "";
        public string Username { get; set; } = "";
        [SQLSize(64)]
        public byte[] Password { get; set; } = new byte[36];
        [SQLSize(64)]
        public byte[] Salt { get; set; } = new byte[36];
        public string Role { get; set; } = "";
    }
}
