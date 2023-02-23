using CSUtil.Web;
using System.Net;
using static CSUtil.Web.Api;

namespace AvaloniaPlannerLib
{
    public static class ApiConsts
    {
        public static ApiResult<string> ExpiredToken = new (HttpStatusCode.NetworkAuthenticationRequired, "!TE!");
        public static ApiResult<string> AccessDenied = new (HttpStatusCode.Unauthorized, "!AD!");
    }
}
