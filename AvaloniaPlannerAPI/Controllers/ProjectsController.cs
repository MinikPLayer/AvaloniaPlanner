using AvaloniaPlannerLib.Data.Auth;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("update_user_projects")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public ActionResult UpdateUserResults(string data)
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var path = Path.Combine(ProjectsPath, authData.Payload!);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".json");
            System.IO.File.WriteAllText(path, data);

            return Ok();
        }

        [HttpGet("get_user_projects")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(string), 200)]
        public ActionResult GetUserProjects()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
                return authData;

            var path = Path.Combine(ProjectsPath, authData.Payload!);
            if (!Directory.Exists(path))
                return Ok();

            var info = new DirectoryInfo(path);
            var files = info.GetFiles().OrderBy(p => p.CreationTime).ToList();
            var response = files.Count > 0 ? System.IO.File.ReadAllText(files[files.Count - 1].FullName) : "";

            return Ok(response);
        }

        [HttpGet("get_user_projects_all_versions")]
        [ProducesResponseType(511)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(List<(DateTime date, string data)>), 200)]
        public ActionResult GetUserProjectsAllVersions()
        {
            var authData = AuthController.AuthUser(Request);
            if (!authData)
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
