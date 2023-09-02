using AvaloniaPlannerLib.Data;
using CSUtil.DB;

namespace AvaloniaPlannerAPI.Data
{
    [SqlTable(TABLE_NAME)]
    [SQLCaseSensitive]
    public class DbAuthToken
    {
        public const string TABLE_NAME = "auth_tokens";

        [SQLPrimary]
        [SQLSize(128)]
        public string Token { get; set; } = "";
        public string User_id { get; set; } = "";
        public DateTime Issue_date { get; set; } = DateTime.MinValue;
        public DateTime Expiration_date { get; set; } = DateTime.MinValue;
        public bool Invalidated { get; set; } = false;

        public DbAuthToken()
        {
            this.Invalidated = true;
        }

        public DbAuthToken(StringID userId)
        {
            this.Token = CSUtil.Crypto.Password.GenerateToken();
            this.User_id = userId;
            this.Issue_date = DateTime.Now;
            this.Expiration_date = this.Issue_date + TimeSpan.FromDays(30);
            this.Invalidated = false;
        }
    }
}
