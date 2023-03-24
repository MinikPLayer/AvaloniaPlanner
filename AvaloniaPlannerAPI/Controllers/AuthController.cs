using AvaloniaPlannerAPI.Data;
using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib;
using AvaloniaPlannerLib.Data.Auth;
using CSUtil.DB;
using CSUtil.Reflection;
using CSUtil.Web;
using Microsoft.AspNetCore.Mvc;
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

        public static DbAuthToken RefreshUserToken(StringID id)
        {
            var invalidToken = new DbAuthToken() { Invalidated = true };
            DbManager.GetDB().Update(
                invalidToken, DbAuthToken.TABLE_NAME, 
                typeof(DbAuthToken).GetProperties().Where(x => x.Name == nameof(DbAuthToken.Invalidated)).ToList(),
                nameof(DbAuthToken.User_id).SQLp(id));

            return AddUserToken(id);
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

        static ApiResult<StringID> AuthUser(string token)
        {
            var authToken = DbManager.GetDB().GetData<DbAuthToken>(
                DbAuthToken.TABLE_NAME,
                nameof(DbAuthToken.Token).SQLp(token)).FirstOrDefault();
            if (authToken == null)
                return new (HttpStatusCode.Unauthorized, "Invalid token");

            if (authToken.Invalidated || DateTime.Now > authToken.Expiration_date)
                return ApiConsts.ExpiredToken.As<StringID>();

            return new (authToken.User_id);

        }

        public static ApiResult<StringID> AuthUser(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Authorization", out var auth))
                return new(HttpStatusCode.Unauthorized, "Authorization token required");

            var splitted = auth.ToString().Split(' ');
            return AuthUser(splitted[^1]);
        }

        public static bool IsUserRole(DbUser user, string role) => user.Role == role;
        public static bool IsUserRole(long userId, string role)
        {
            var user = GetDbUserById(userId);
            if (user == null)
                return false;

            return user.Role == role;
        }

        [HttpPost("login")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ApiAuthToken), 200)]
        public ActionResult Login(string login, string password)
        {
            var auth = AuthUser(login, password);
            if (!auth)
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
            var token = RefreshUserToken(newUser.Id);

            return Ok(ClassCopier.Create<ApiAuthToken>(token));
        }

        [HttpPost("invalidate_tokens")]
        public ActionResult InvalidateTokens(string token)
        {
            var auth = AuthUser(token);
            if (!auth)
                return auth;

            var newToken = RefreshUserToken(auth.Payload);
            var apiToken = new ApiAuthToken();
            ClassCopier.Copy(newToken, apiToken);

            return Ok(apiToken);
        }
    }
}
