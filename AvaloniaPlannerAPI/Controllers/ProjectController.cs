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
        public static List<DbProjectBin> GetProjectBinsDB(StringID projectId) => DbManager.GetDB().GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Position), nameof(DbProjectBin.Project_id).SQLp(projectId));
        public static List<DbProjectTask> GetTasksDB(StringID binId) => DbManager.GetDB().GetData<DbProjectTask>(
            DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Bin_id).SQLp(binId));

        public static DbProject? GetProjectDB(StringID projectId) => DbManager.GetDB().GetData<DbProject>(
            DbProject.TABLE_NAME, nameof(DbProject.Id).SQLp(projectId)).FirstOrDefault();
        public static DbProjectBin? GetProjectBinDB(StringID binId) => DbManager.GetDB().GetData<DbProjectBin>(
            DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id).SQLp(binId)).FirstOrDefault();
        public static DbProjectPermissions? GetProjectPermissionsDB(StringID userId, StringID projectId) => DbManager.GetDB().GetData<DbProjectPermissions>(
            DbProjectPermissions.TABLE_NAME, nameof(DbProjectPermissions.issue_date) + " DESC", nameof(DbProjectPermissions.project_id).SQLp(projectId), nameof(DbProjectPermissions.user_id).SQLp(userId)).FirstOrDefault();
        
        public static ProjectStatus GetProjectStatus(StringID projectId)
        {
            var status = DbManager.GetDB().GetData<DbProjectStatus>(DbProjectStatus.TABLE_NAME, nameof(DbProjectStatus.date) + " DESC", nameof(DbProjectStatus.project_id).SQLp(projectId)).FirstOrDefault();
            return status == null ? ProjectStatus.Unknown : status.status;
        }

        public static ProjectStatus GetTaskStatus(StringID taskID)
        {
            var status = DbManager.GetDB().GetData<DbTaskStatus>(DbTaskStatus.TABLE_NAME, nameof(DbTaskStatus.date) + " DESC", nameof(DbTaskStatus.task_id).SQLp(taskID)).FirstOrDefault();
            return status == null ? ProjectStatus.Unknown : status.status;
        }

        public static bool CanUserWrite(StringID userId, StringID projectId)
        {
            var perms = GetProjectPermissionsDB(userId, projectId);
            return perms != null && perms.can_write;
        }

        public static bool CanUserRead(StringID userId, StringID projectId)
        {
            var perms = GetProjectPermissionsDB(userId, projectId);
            return perms != null && perms.can_read;
        }

        [HttpPost("create_new_project")]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(typeof(ApiProject), 200)]
        public ActionResult CreateProject(string name, string description, ProjectStatus status)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var existingProject = this.GetDB().Count(DbProject.TABLE_NAME, nameof(DbProject.Name).SQLp(name));
            if (existingProject > 0)
                return Conflict("Project with this name already exists");

            var newProject = new DbProject(this.GetDB(), name, description, authData.Payload);
            this.GetDB().InsertData(newProject, DbProject.TABLE_NAME);
            this.GetDB().InsertData(new DbProjectStatus(this.GetDB(), newProject.Id, status), DbProjectStatus.TABLE_NAME);

            var perms = DbProjectPermissions.All(this.GetDB(), newProject.Id, authData.Payload);
            this.GetDB().InsertData(perms, DbProjectPermissions.TABLE_NAME);

            return Ok(ClassCopier.Create<ApiProject>(newProject));
        }

        [HttpPost("update_project_status")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectStatus), 200)]
        public ActionResult UpdateProjectStatus(string projectId, ProjectStatus status)
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

            return Ok(ClassCopier.Create<ApiProjectStatus>(newStatus));
        }

        [HttpPost("update_project_info")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProject), 200)]
        public ActionResult UpdateProjectInfo(string id, string name, string description)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            if (!CanUserWrite(authData.Payload, id))
                return ApiConsts.AccessDenied;

            var existingProject = GetProjectDB(id);
            if (existingProject == null)
                return NotFound("Project not found");

            var dbProject = new DbProject();
            ClassCopier.CopySingle(existingProject, dbProject);
            dbProject.Name = name;
            dbProject.Description = description;
            dbProject.LastUpdate = DateTime.Now;
            this.GetDB().Update(dbProject, DbProject.TABLE_NAME, nameof(DbProject.Id).SQLp(id));

            return Ok(ClassCopier.Create<ApiProject>(dbProject));
        }

        [HttpGet("get_all_projects")]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(List<ApiProject>), 200)]
        public ActionResult GetAllProjects()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var projects = GetAllprojectsDB();
            projects.RemoveAll(x => !CanUserRead(authData.Payload, x.Id));
            return Ok(ClassCopier.CreateList<DbProject, ApiProject>(projects));
        }

        [HttpGet("get_full_project_data")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProject), 200)]
        public ActionResult GetFullResultData(string id)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            if (!CanUserRead(authData.Payload, id))
                return Forbid($"Access Denied");

            var dbProject = GetProjectDB(id);
            if (dbProject == null)
                return NotFound("Project not found");

            var project = ClassCopier.Create<ApiProject>(dbProject);
            project.Bins = ClassCopier.CreateList<DbProjectBin, ApiProjectBin>(GetProjectBinsDB(id));
            foreach (var bin in project.Bins)
                bin.Tasks = ClassCopier.CreateList<DbProjectTask, ApiProjectTask>(GetTasksDB(bin.Id));

            return Ok(project);
        }

        [HttpGet("get_project_bins")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(List<ApiProjectBin>), 200)]
        public ActionResult GetProjectBins(string id)
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

        [HttpPost("create_project_bin")]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(typeof(ApiProjectBin), 200)]
        public ActionResult CreateProjectBin(string projectId, string name)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            if (!CanUserWrite(authData.Payload, projectId))
                return ApiConsts.AccessDenied;

            var existingBins = this.GetDB().Count(DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Name).SQLp(name), nameof(DbProjectBin.Project_id).SQLp(projectId));
            if (existingBins > 0)
                return Conflict("Bin with this name already exists in this project");

            var lastBin = this.GetDB().GetDataLimit<DbProjectBin>(DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Position) + " DESC", 1).FirstOrDefault();

            var newBin = new DbProjectBin();
            newBin.Project_id = projectId;
            newBin.Name = name;
            newBin.Id = this.GetDB().GenerateUniqueIdString(DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id));
            newBin.Position = lastBin == null ? 0 : lastBin.Position + 1;

            this.GetDB().InsertData(newBin, DbProjectBin.TABLE_NAME);
            return Ok(ClassCopier.Create<ApiProjectBin>(newBin));
        }

        [HttpPost("update_project_bin_name")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectBin), 200)]
        public ActionResult UpdateProjectBinName(string binId, string newName)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var bin = GetProjectBinDB(binId);
            if (bin == null)
                return NotFound("Project bin not found");

            if (!CanUserWrite(authData.Payload, bin.Project_id))
                return ApiConsts.AccessDenied;

            bin.Name = newName;
            this.GetDB().Update(bin, DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id).SQLp(binId));

            return Ok(ClassCopier.Create<ApiProjectBin>(bin));
        }

        [HttpPost("change_project_bin_archive_status")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectBin), 200)]
        public ActionResult ChangeProjectBinArchiveStatus(string binId, bool status)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var bin = GetProjectBinDB(binId);
            if (bin == null)
                return NotFound("Project bin not found");

            if (!CanUserWrite(authData.Payload, bin.Project_id))
                return ApiConsts.AccessDenied;

            bin.Archived = status;
            this.GetDB().Update(bin, DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id).SQLp(binId));

            return Ok(ClassCopier.Create<ApiProjectBin>(bin));
        }

        [HttpPost("change_project_bin_position")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectBin), 200)]
        public ActionResult ChangeProjectBinPosition(string binId, int position)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var bin = GetProjectBinDB(binId);
            if (bin == null)
                return NotFound("Project bin not found");

            if (!CanUserWrite(authData.Payload, bin.Project_id))
                return ApiConsts.AccessDenied;

            var allBins = this.GetDB().GetData<DbProjectBin>(DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Position), nameof(DbProjectBin.Project_id).SQLp(bin.Project_id));
            allBins.RemoveAll(x => x.Id == binId);
            position = Math.Clamp(position, 0, allBins.Count);
            allBins.Insert(position, bin);

            for (var i = 0; i < allBins.Count; i++)
            {
                if (allBins[i].Position != i)
                {
                    allBins[i].Position = i;
                    this.GetDB().Update(allBins[i], DbProjectBin.TABLE_NAME, nameof(DbProjectBin.Id).SQLp(allBins[i].Id));
                }
            }

            return Ok(ClassCopier.CreateList<DbProjectBin, ApiProjectBin>(allBins));
        }

        [HttpPost("create_new_task")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectTask), 200)]
        public ActionResult CreateNewTask(string binId, string name, string description, ProjectStatus status, int priority)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var bin = GetProjectBinDB(binId);
            if (bin == null)
                return NotFound("Project bin not found");

            if (!CanUserWrite(authData.Payload, bin.Project_id))
                return ApiConsts.AccessDenied;

            var newTask = new DbProjectTask(this.GetDB(), bin.Project_id, binId, name, description, priority);
            this.GetDB().InsertData(newTask, DbProjectTask.TABLE_NAME);

            var newTaskStatus = new DbTaskStatus(this.GetDB(), newTask.Id, status);
            this.GetDB().InsertData(newTaskStatus, DbTaskStatus.TABLE_NAME);

            return Ok(newTask);
        }

        [HttpPost("update_task_info")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectTask), 200)]
        public ActionResult UpdateTaskInfo(string taskId, string? name = null, string? description = null, int? priority = null)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var task = this.GetDB().GetData<DbProjectTask>(DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Id).SQLp(taskId)).FirstOrDefault();
            if (task == null)
                return NotFound("Task not found");

            if (!CanUserWrite(authData.Payload, task.Project_id))
                return ApiConsts.AccessDenied;

            if (!string.IsNullOrEmpty(name))
                task.Name = name;

            if (!string.IsNullOrEmpty(description))
                task.Description = description;

            if (priority != null)
                task.Priority = priority.Value;

            this.GetDB().Update(task, DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Id).SQLp(task.Id));

            return Ok(ClassCopier.Create<ApiProjectTask>(task));
        }

        [HttpPost("update_task_status")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ApiProjectTask), 200)]
        public ActionResult UpdateTaskStatus(string taskId, ProjectStatus status)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var task = this.GetDB().GetData<DbProjectTask>(DbProjectTask.TABLE_NAME, nameof(DbProjectTask.Id).SQLp(taskId)).FirstOrDefault();
            if (task == null)
                return NotFound("Task not found");

            if (!CanUserWrite(authData.Payload, task.Project_id))
                return ApiConsts.AccessDenied;

            var newStatus = new DbTaskStatus(this.GetDB(), taskId, status);
            this.GetDB().InsertData(newStatus, DbTaskStatus.TABLE_NAME);

            var apiTask = ClassCopier.Create<ApiProjectTask>(task);
            apiTask.status = status;

            return Ok(apiTask);
        }

        [HttpGet("get_tasks")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(List<ApiProjectTask>), 200)]
        public ActionResult GetTasks(string bin_id)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var tasks = GetTasksDB(bin_id);
            if(tasks.Count > 0 && !CanUserRead(authData.Payload, tasks[0].Project_id))
                return Forbid($"No permission to read from project");
            
            ClassCopier.CopyList(tasks, out List<ApiProjectTask> apiTasks);
            foreach (var task in apiTasks)
                task.status = GetTaskStatus(task.Id);

            return Ok(apiTasks);
        }
    }
}
