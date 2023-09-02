using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Auth
{
    public class ApiAuthToken
    {
        public string Token { get; set; } = "";
        public long User_id { get; set; } = -1;
        public DateTime Issue_date { get; set; } = DateTime.MinValue;
        public DateTime Expiration_date { get; set; } = DateTime.MinValue;
        public bool Invalidated { get; set; } = false;
    }
}
