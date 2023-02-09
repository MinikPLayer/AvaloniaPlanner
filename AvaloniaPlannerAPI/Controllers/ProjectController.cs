using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib.Data.Project;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AvaloniaPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        public static List<Project> GetAllprojectsDB() => DbManager.DB!.GetData<Project>("Projects");

        [HttpGet]
        public ActionResult GetAllProjects()
        {
            return Ok(GetAllprojectsDB());
        }
    }
}
