using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectBusiness _projectService;

        public ProjectController(IProjectBusiness projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectService.GetAllProjects();
            return Ok(projects);
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProject(long projectId)
        {
            var project = await _projectService.GetProject(projectId);
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto dto)
        {
            var project = await _projectService.CreateProject(dto);
            return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
        }

        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(long projectId, [FromBody] ProjectDto dto)
        {
            var updated = await _projectService.UpdateProject(projectId, dto);
            return Ok(updated);
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(long projectId)
        {
            await _projectService.DeleteProject(projectId);
            return NoContent();
        }
    }
}

