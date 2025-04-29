using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectBusiness _projectBusiness;

        public ProjectController(IProjectBusiness projectBusiness)
        {
            _projectBusiness = projectBusiness;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectBusiness.GetAllProjects();
            return Ok(projects);
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProject(long projectId)
        {
            var project = await _projectBusiness.GetProject(projectId);
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            var createdProject = await _projectBusiness.CreateProject(project);
            return CreatedAtAction(nameof(GetProject), new { projectId = createdProject.Id }, createdProject);
        }

        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(long projectId, [FromBody] Project project)
        {
            var updatedProject = await _projectBusiness.UpdateProject(projectId, project);
            return Ok(updatedProject);
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(long projectId)
        {
            await _projectBusiness.DeleteProject(projectId);
            return NoContent();
        }
    }
}