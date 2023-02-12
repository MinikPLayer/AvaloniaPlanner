using AvaloniaPlannerLib.Data;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data
{
    [SqlTable(TABLE_NAME)]
    public class DbAuthToken
    {
        public const string TABLE_NAME = "auth_tokens";

        [SQLPrimary]
        [SQLSize(64)]
        public string Token { get; set; } = "";
        public long User_id { get; set; } = -1;
        public DateTime Issue_date { get; set; } = DateTime.MinValue;
        public DateTime Expiration_date { get; set; } = DateTime.MinValue;
        public bool Invalidated { get; set; } = false;

        public DbAuthToken()
        {
            this.Invalidated = true;
        }

        public DbAuthToken(long userId)
        {
            this.Token = CSUtil.Crypto.Password.GenerateToken(64);
            this.User_id = userId;
            this.Issue_date = DateTime.Now;
            this.Expiration_date = this.Issue_date + TimeSpan.FromDays(30);
            this.Invalidated = false;
        }
    }
}
