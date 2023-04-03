using AvaloniaPlannerAPI.Data;
using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib;
using AvaloniaPlannerLib.Data.Auth;
using CSUtil.DB;
using CSUtil.Reflection;
using CSUtil.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AvaloniaPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static DbUser? GetDbUser(string login) => DbManager.GetDB().GetData<DbUser>(DbUser.TABLE_NAME, nameof(DbUser.Login).SQLp(login)).FirstOrDefault();
        public static DbUser? GetDbUserById(long id) => DbManager.GetDB().GetData<DbUser>(
            DbUser.TABLE_NAME, nameof(DbUser.Id).SQLp(id)).FirstOrDefault();

        public static DbAuthToken AddUserToken(StringID id)
        {
            var newToken = new DbAuthToken(id);
            DbManager.GetDB().InsertData(newToken, DbAuthToken.TABLE_NAME);

            return newToken;
        }

        /// <summary>
        /// Invalidates all user tokens, and creates a new one
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>A new token</returns>
        public static DbAuthToken ResetUserTokens(StringID id)
        {
            var invalidToken = new DbAuthToken() { Invalidated = true };
            DbManager.GetDB().Update(
                invalidToken, DbAuthToken.TABLE_NAME, 
                typeof(DbAuthToken).GetProperties().Where(x => x.Name == nameof(DbAuthToken.Invalidated)).ToList(),
                nameof(DbAuthToken.User_id).SQLp(id));

            return AddUserToken(id);
        }

        public static bool InvalidateUserToken(StringID token)
        {
            var authToken = DbManager.GetDB().GetData<DbAuthToken>(DbAuthToken.TABLE_NAME, nameof(DbAuthToken.Token).SQLp(token)).FirstOrDefault();
            if (authToken == null)
                return false;

            authToken.Invalidated = true;
            DbManager.GetDB().Update(
                authToken, DbAuthToken.TABLE_NAME, 
                typeof(DbAuthToken).GetProperties().Where(x => x.Name == nameof(DbAuthToken.Invalidated)).ToList(), 
                nameof(DbAuthToken.Token).SQLp(token));
            return true;
        }

        public static ApiResult<DbUser> AuthUser(string login, string password)
        {
            var user = GetDbUser(login);
            if (user == null)
                return new ApiResult<DbUser>(HttpStatusCode.NotFound, "User not found");

            var hashedPassword = CSUtil.Crypto.Password.GetPasswordHash(password, user.Salt);
            var passwordsMatch = CSUtil.Crypto.Password.ComparePasswords(hashedPassword.hash, user.Password);
            if (!passwordsMatch)
                return new ApiResult<DbUser>(HttpStatusCode.Unauthorized, "Authentication failed - bad password", user);

            return new ApiResult<DbUser>(user);
        }

        const string AuthInvalidatedTokenMessage = "!#TI#!";
        const string AuthExpiredTokenMessage = "!#TE#!";
        static ApiResult<StringID> AuthUser(string? token)
        {
            if(string.IsNullOrEmpty(token))
                return new(HttpStatusCode.Unauthorized, "Authorization token required");

            var authToken = DbManager.GetDB().GetData<DbAuthToken>(
                DbAuthToken.TABLE_NAME,
                nameof(DbAuthToken.Token).SQLp(token)).FirstOrDefault();

            if (authToken == null)
                return new(HttpStatusCode.Unauthorized, "Invalid token");

            if (authToken.Invalidated)
                return ApiConsts.InvalidatedToken.As<StringID>();

            if(DateTime.Now > authToken.Expiration_date)
            {
                var ret = ApiConsts.ExpiredToken.As<StringID>();
                ret.Payload = authToken.User_id;
                return ret;
            }

            return new(authToken.User_id);

        }

        public static ApiResult<StringID> AuthUser(HttpRequest request) => AuthUser(ExtractBearer(request));

        public static string? ExtractBearer(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Authorization", out var auth))
                return null;

            var splitted = auth.ToString().Split(' ');
            return splitted[^1];
        }

        public static bool IsUserRole(DbUser user, string role) => user.Role == role;
        public static bool IsUserRole(long userId, string role)
        {
            var user = GetDbUserById(userId);
            if (user == null)
                return false;

            return user.Role == role;
        }

        [HttpGet("test_connection")]
        [ProducesResponseType(200)]
        public ActionResult TestConnection() => Ok("OK");

        [HttpGet("get_user_id")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(string), 200)]
        public ActionResult GetUserId()
        {
            var auth = AuthUser(Request);
            if (!auth.IsOk())
                return auth;

            return Ok(auth.Payload);
        }

        [HttpPost("login")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ApiAuthToken), 200)]
        public ActionResult Login(string login, string password)
        {
            var auth = AuthUser(login, password);
            if (!auth.IsOk())
                return auth;

            var user = auth.Payload;
            var token = AddUserToken(user.Id);
            return Ok(ClassCopier.Create<ApiAuthToken>(token));
        }

        [HttpPost("register")]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ApiAuthToken), 200)]
        public ActionResult Register(string login, string password, string username, string email)
        {
            if (GetDbUser(login) != null || 
                this.GetDB().GetData<DbUser>(DbUser.TABLE_NAME, nameof(DbUser.Username).SQLp(username)).FirstOrDefault() != null)
                return BadRequest("You are already registered");

            var newUser = new DbUser();
            newUser.Id = this.GetDB().GenerateUniqueIdString(DbUser.TABLE_NAME, nameof(DbUser.Id));
            newUser.Login = login;
            newUser.Username = username;

            var pass = CSUtil.Crypto.Password.GetPasswordHash(password, CSUtil.Crypto.Password.GenerateSalt());
            newUser.Password = pass.hash;
            newUser.Salt = pass.salt;

            this.GetDB().InsertData(newUser, "users");
            var token = ResetUserTokens(newUser.Id);

            return Ok(ClassCopier.Create<ApiAuthToken>(token));
        }

        [HttpPost("refresh_token")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ApiAuthToken), 200)]
        public ActionResult RefreshToken()
        {
            var auth = AuthUser(Request);
            if(auth.IsOk() || auth.IsExpired())
            {
                var token = ExtractBearer(Request);
                if(string.IsNullOrEmpty(token))
                    return BadRequest("Token required");

                var invalidated = InvalidateUserToken(token);
                if(!invalidated)
                    return BadRequest("Invalid token");

                var newToken = AddUserToken(auth.Payload);
                var apiToken = new ApiAuthToken();
                ClassCopier.Copy(newToken, apiToken);
                return Ok(apiToken);
            }

            return auth;
        }

        [HttpPost("invalidate_tokens")]
        public ActionResult InvalidateTokens(string token)
        {
            var auth = AuthUser(token);
            if (!auth.IsOk())
                return auth;

            var newToken = ResetUserTokens(auth.Payload);
            var apiToken = new ApiAuthToken();
            ClassCopier.Copy(newToken, apiToken);

            return Ok(apiToken);
        }
    }
}
