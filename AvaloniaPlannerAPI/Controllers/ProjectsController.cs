using AvaloniaPlannerLib.Data.Auth;
using AvaloniaPlannerLib.Data.Project;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvaloniaPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        public static string ProjectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AvPlanner", "Server");

        static ProjectController()
        {
            Directory.CreateDirectory(ProjectsPath);
        }

        static string? ReadLastProjectSave(string user)
        {
            var path = Path.Combine(ProjectsPath, user);
            if (!Directory.Exists(path))
                return null;

            var info = new DirectoryInfo(path);
            var files = info.GetFiles().OrderBy(p => p.CreationTime).ToList();
            var response = files.Count > 0 ? System.IO.File.ReadAllText(files[files.Count - 1].FullName) : null;

            return response;
        }

        [HttpPost("update_user_projects")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public ActionResult UpdateUserProjects([FromBody] string data)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData.IsOk())
                return authData;

            var path = Path.Combine(ProjectsPath, authData.Payload!);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".json");
            System.IO.File.WriteAllText(path, data);

            return Ok();
        }

        [HttpGet("get_last_modification_date")]
        [ProducesResponseType(511)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(DateTime), 200)]
        public ActionResult GetLastModificationDate()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData.IsOk())
                return authData;

            var data = ReadLastProjectSave(authData.Payload!);
            if (data == null)
                return Ok(DateTime.MinValue);

            var projects = JsonConvert.DeserializeObject<List<ApiProject>>(data);
            if (projects == null)
                return StatusCode(500, "Cannot deserialize saved projects");

            if(projects.Count == 0)
                return Ok(DateTime.MinValue);

            var lastMod = projects.Select(x => x.LastUpdate).OrderByDescending(x => x).FirstOrDefault();
            return Ok(lastMod);
        }

        [HttpGet("get_user_projects")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(string), 200)]
        public ActionResult GetUserProjects()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData.IsOk())
                return authData;

            var data = ReadLastProjectSave(authData.Payload!);
            if (data == null)
                return Ok("");

            return Ok(data);
        }

        [HttpGet("get_user_projects_all_versions")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(List<(DateTime date, string data)>), 200)]
        public ActionResult GetUserProjectsAllVersions()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData.IsOk())
                return authData;

            var path = Path.Combine(ProjectsPath, authData.Payload!);
            if (!Directory.Exists(path))
                return Ok();

            var data = new List<(DateTime date, string data)>();
            var files = new DirectoryInfo(path).GetFiles();
            foreach (var file in files)
                data.Add((file.CreationTime, System.IO.File.ReadAllText(file.FullName)));

            return Ok(data);
        }
    }
}
