using AvaloniaPlannerAPI.Data.Project;
using AvaloniaPlannerAPI.Managers;
using AvaloniaPlannerLib;
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
        public static List<DbProjectBin> GetProjectBinsDB(long projectId) => DbManager.DB!.GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Project_id).SQLp(projectId));
        public static List<DbProjectTask> GetTasksDB(long binId) => DbManager.DB!.GetData<DbProjectTask>(
            DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Bin_id).SQLp(binId));

        public static DbProject? GetProjectDB(long projectId) => DbManager.DB!.GetData<DbProject>(
            DbProject.TABLE_NAME, nameof(DbProject.Id).SQLp(projectId)).FirstOrDefault();
        public static DbProjectBin? GetProjectBinDB(long binId) => DbManager.DB!.GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id).SQLp(binId)).FirstOrDefault();
        public static DbProjectPermissions? GetProjectPermissionsDB(long userId, long projectId) => DbManager.DB!.GetData<DbProjectPermissions>(
            DbProjectPermissions.TABLE_NAME, nameof(DbProjectPermissions.issue_date) + " DESC", nameof(DbProjectPermissions.project_id).SQLp(projectId), nameof(DbProjectPermissions.user_id).SQLp(userId)).FirstOrDefault();

        public static bool CanUserWrite(long userId, long projectId)
        {
            var perms = GetProjectPermissionsDB(userId, projectId);
            return perms != null && perms.can_write;
        }

        public static bool CanUserRead(long userId, long projectId)
        {
            var perms = GetProjectPermissionsDB(userId, projectId);
            return perms != null && perms.can_read;
        }

        [HttpPost("create_new_project")]
        public ActionResult CreateProject(string name, string description, ProjectStatus status)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var existingProject = DbManager.DB!.Count(DbProject.TABLE_NAME, nameof(DbProject.Name).SQLp(name));
            if (existingProject > 0)
                return Conflict("Project with this name already exists");

            var id = DbManager.DB!.GenerateUniqueIdLong(DbProject.TABLE_NAME, nameof(DbProject.Id));
            var newProject = new DbProject(id, name, description, authData.Payload, status);
            DbManager.DB!.InsertData(newProject, DbProject.TABLE_NAME);

            var perms = DbProjectPermissions.All(DbManager.DB!, id, authData.Payload);
            DbManager.DB!.InsertData(perms, DbProjectPermissions.TABLE_NAME);

            return Ok(ClassCopier.Create<ApiProject>(newProject));
        }

        [HttpDelete("archive_project")]
        public ActionResult ArchiveProject(long id)
        {

        }

        [HttpPost("update_project_info")]
        public ActionResult UpdateProjectInfo(ApiProject projectInfo)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            if (!CanUserWrite(authData.Payload, projectInfo.Id))
                return ApiConsts.AccessDenied;

            var dbProject = ClassCopier.Create<DbProject>(projectInfo);
            DbManager.DB!.Update(dbProject, DbProject.TABLE_NAME, nameof(DbProject.Id).SQLp(projectInfo.Id));

            return Ok(projectInfo);
        }

        [HttpGet("get_all_projects")]
        public ActionResult GetAllProjects()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var projects = GetAllprojectsDB();
            projects.RemoveAll(x => !CanUserRead(authData.Payload, x.Id));
            return Ok(ClassCopier.CreateList<DbProject, ApiProject>(projects));
        }

        [HttpGet("get_project_bins")]
        public ActionResult GetProjectBins(long id)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            if (!CanUserRead(authData.Payload, id))
                return Forbid($"Access Denied");

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
            if(tasks.Count > 0 && !CanUserRead(authData.Payload, tasks[0].Project_id))
                return Forbid($"No permission to read from project");
            
            ClassCopier.CopyList(tasks, out List<ApiProjectTask> apiTasks);
            return Ok(apiTasks);
        }
    }
}
