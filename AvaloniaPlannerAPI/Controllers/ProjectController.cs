using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.DB;
using CSUtil.Reflection;
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
        public static List<DbProject> GetAllprojectsDB() => DbManager.DB!.GetData<DbProject>(DbProject.TABLE_NAME);
        public static List<DbProjectBin> GetProjectBinsDB(long id) => DbManager.DB!.GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Project_id).SQLp(id));
        public static List<DbProjectTask> GetTasksDB(long binId) => DbManager.DB!.GetData<DbProjectTask>(
            DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Bin_id).SQLp(binId));

        // TODO: Add support for limiting access to projects
        public static bool CanUserWrite(long userId, long projectId) => true;
        public static bool CanUserRead(long userId, long projectId) => true;

        [HttpGet("get_all_projects")]
        public ActionResult GetAllProjects()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var projects = GetAllprojectsDB();
            ClassCopier.CopyList(projects, out List<ApiProject> apiProjects);
            return Ok(apiProjects);
        }

        [HttpGet("get_project_bins")]
        public ActionResult GetProjectBins(long id)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var bins = GetProjectBinsDB(id);
            ClassCopier.CopyList(bins, out List<ApiProjectBin> apiBins);
            return Ok(apiBins);
        }

        [HttpGet("get_tasks")]
        public ActionResult GetTasks(long bin_id)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var tasks = GetTasksDB(bin_id);
            ClassCopier.CopyList(tasks, out List<ApiProject> apiTasks);

            return Ok(apiTasks);
        }
    }
}
