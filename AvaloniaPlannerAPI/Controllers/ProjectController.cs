using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib.Data.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

namespace AvaloniaPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        public static List<DbProject> GetAllprojectsDB() => DbManager.DB!.GetData<DbProject>("Projects");

        [HttpGet("get_all_projects")]
        public ActionResult GetAllProjects(string token)
        {
            var authData = AuthController.AuthUser(token);
            if (!authData)
                return authData;

            return Ok(GetAllprojectsDB());
        }
    }
}
