using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib
{
    public static class ApiConsts
    {
        public static (HttpStatusCode code, string msg) ExpiredToken = (HttpStatusCode.NetworkAuthenticationRequired, "!TE");
    }
}
