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
        public static List<DbProject> GetAllprojectsDB() => DbManager.GetDB().GetData<DbProject>(DbProject.TABLE_NAME);
        public static List<DbProjectBin> GetProjectBinsDB(long projectId) => DbManager.GetDB().GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Project_id).SQLp(projectId));
        public static List<DbProjectTask> GetTasksDB(long binId) => DbManager.GetDB().GetData<DbProjectTask>(
            DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Bin_id).SQLp(binId));

        public static DbProject? GetProjectDB(long projectId) => DbManager.GetDB().GetData<DbProject>(
            DbProject.TABLE_NAME, nameof(DbProject.Id).SQLp(projectId)).FirstOrDefault();
        public static DbProjectBin? GetProjectBinDB(long binId) => DbManager.GetDB().GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id).SQLp(binId)).FirstOrDefault();
        public static DbProjectPermissions? GetProjectPermissionsDB(long userId, long projectId) => DbManager.GetDB().GetData<DbProjectPermissions>(
            DbProjectPermissions.TABLE_NAME, nameof(DbProjectPermissions.issue_date) + " DESC", nameof(DbProjectPermissions.project_id).SQLp(projectId), nameof(DbProjectPermissions.user_id).SQLp(userId)).FirstOrDefault();
        
        public static ProjectStatus GetProjectStatus(long projectId)
        {
            var status = DbManager.GetDB().GetData<DbProjectStatus>(DbProjectStatus.TABLE_NAME, nameof(DbProjectStatus.date) + " DESC", nameof(DbProjectStatus.project_id).SQLp(projectId)).FirstOrDefault();
            return status == null ? ProjectStatus.Unknown : status.status;
        }

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

            var existingProject = this.GetDB().Count(DbProject.TABLE_NAME, nameof(DbProject.Name).SQLp(name));
            if (existingProject > 0)
                return Conflict("Project with this name already exists");

            var id = this.GetDB().GenerateUniqueIdLong(DbProject.TABLE_NAME, nameof(DbProject.Id));
            var newProject = new DbProject(id, name, description, authData.Payload);
            this.GetDB().InsertData(newProject, DbProject.TABLE_NAME);
            this.GetDB().InsertData(new DbProjectStatus(this.GetDB(), id, status), DbProjectStatus.TABLE_NAME);

            var perms = DbProjectPermissions.All(this.GetDB(), id, authData.Payload);
            this.GetDB().InsertData(perms, DbProjectPermissions.TABLE_NAME);

            return Ok(ClassCopier.Create<ApiProject>(newProject));
        }

        [HttpDelete("update_project_status")]
        public ActionResult UpdateStatus(long projectId, ProjectStatus status)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            if (!CanUserWrite(authData.Payload, projectId))
                return ApiConsts.AccessDenied;

            var project = GetProjectDB(projectId);
            if (project == null)
                return NotFound("Project not found");

            var newStatus = new DbProjectStatus(this.GetDB(), projectId, status);
            this.GetDB().InsertData(newStatus, DbProjectStatus.TABLE_NAME);

            return Ok(newStatus);
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
            this.GetDB().Update(dbProject, DbProject.TABLE_NAME, nameof(DbProject.Id).SQLp(projectInfo.Id));

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
