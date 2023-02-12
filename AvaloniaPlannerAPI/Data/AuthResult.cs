using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AvaloniaPlannerAPI.Data
{
    public class AuthResult<T>
    {
        public HttpStatusCode StatusCode;
        public string? Message = null;
        public T? Payload;

        public static implicit operator bool(AuthResult<T> ret) => ret.StatusCode == HttpStatusCode.OK;

        public static implicit operator ActionResult(AuthResult<T> ret)
        {
            if (ret)
                return new ObjectResult(ret.Message) { StatusCode = (int)ret.StatusCode };

            return new ObjectResult(ret.Message) { StatusCode = (int)ret.StatusCode };
        }

        public AuthResult(T payload)
        {
            this.StatusCode = HttpStatusCode.OK;
            this.Payload = payload;
        }

        public AuthResult(HttpStatusCode code, string message, T? payload = default(T))
        {
            this.StatusCode = code;
            this.Message = message;
            this.Payload = payload;
        }
    }
}
