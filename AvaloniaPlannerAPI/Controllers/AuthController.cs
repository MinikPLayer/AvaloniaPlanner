using AvaloniaPlannerAPI.Data;
using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib;
using AvaloniaPlannerLib.Data.Auth;
using CSUtil.DB;
using CSUtil.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace AvaloniaPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static DbUser? GetDbUser(string login) => DbManager.DB!.GetData<DbUser>("users", nameof(DbUser.Login).SQLp(login)).FirstOrDefault();
        public static DbAuthToken AddUserToken(long id)
        {
            var newToken = new DbAuthToken(id);
            DbManager.DB!.InsertData(newToken, DbAuthToken.TABLE_NAME);

            return newToken;
        }

        public static DbAuthToken RefreshUserToken(long id)
        {
            var invalidToken = new DbAuthToken() { Invalidated = true };
            DbManager.DB!.Update(
                invalidToken, DbAuthToken.TABLE_NAME, 
                typeof(DbAuthToken).GetProperties().Where(x => x.Name == nameof(DbAuthToken.Invalidated)).ToList(),
                nameof(DbAuthToken.User_id).SQLp(id));

            return AddUserToken(id);
        }

        public static AuthResult<DbUser> AuthUser(string login, string password)
        {
            var user = GetDbUser(login);
            if (user == null)
                return new AuthResult<DbUser>(HttpStatusCode.NotFound, "User not found");

            var hashedPassword = CSUtil.Crypto.Password.GetPasswordHash(password, user.Salt);
            var passwordsMatch = CSUtil.Crypto.Password.ComparePasswords(hashedPassword.hash, user.Password);
            if (!passwordsMatch)
                return new AuthResult<DbUser>(HttpStatusCode.Unauthorized, "Authentication failed - bad password", user);

            return new AuthResult<DbUser>(user);
        }

        public static AuthResult<long> AuthUser(string token)
        {
            var authToken = DbManager.DB!.GetData<DbAuthToken>(
                DbAuthToken.TABLE_NAME,
                nameof(DbAuthToken.Token).SQLp(token)).FirstOrDefault();
            if (authToken == null)
                return new (HttpStatusCode.NotFound, "User not found");

            if (authToken.Invalidated || DateTime.Now > authToken.Expiration_date)
                return new (ApiConsts.ExpiredToken.code, ApiConsts.ExpiredToken.msg);

            return new (authToken.User_id);

        }

        [HttpPost("login")]
        public ActionResult Login(string login, string password)
        {
            var auth = AuthUser(login, password);
            if (!auth)
                return auth;

            var user = auth.Payload;
            var token = AddUserToken(user.Id);
            return Ok(token);
        }

        [HttpPost("register")]
        public ActionResult Register(string login, [FromBody] string password, string username, string email)
        {
            if (GetDbUser(login) != null || 
                DbManager.DB!.GetData<DbUser>("users", nameof(DbUser.Username).SQLp(username)).FirstOrDefault() != null)
                return BadRequest("You are already registered");

            var newUser = new DbUser();
            newUser.Id = DbManager.DB.GenerateUniqueId("users", nameof(DbUser.Id));
            newUser.Login = login;
            newUser.Username = username;

            var pass = CSUtil.Crypto.Password.GetPasswordHash(password, CSUtil.Crypto.Password.GenerateSalt());
            newUser.Password = pass.hash;
            newUser.Salt = pass.salt;

            DbManager.DB.InsertData(newUser, "users");
            var token = RefreshUserToken(newUser.Id);

            return Ok(token);
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
