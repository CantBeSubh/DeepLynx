using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBusiness _roleBusiness;

        public RoleController(IRoleBusiness roleBusiness)
        {
            _roleBusiness = roleBusiness;
        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles(long projectId)
        {
            try
            {
                var roles = await _roleBusiness.GetAllRoles(projectId);
                return Ok(new { message = "Returned all roles", data = roles });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing roles: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpGet("GetRole/{roleId}")]
        public async Task<IActionResult> GetRole(long projectId, long roleId)
        {
            try
            {
                var role = await _roleBusiness.GetRole(projectId, roleId);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving role {roleId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(
            long projectId, 
            [FromBody] RoleRequestDto dto)
        {
            try
            {
                var role = await _roleBusiness.CreateRole(projectId, dto);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating role: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }   

        [HttpPut("UpdateRole/{roleId}")]
        public async Task<IActionResult> UpdateRole(
            long projectId, 
            long roleId, 
            [FromBody] RoleRequestDto dto)
        {
            try
            {
                var role = await _roleBusiness.UpdateRole(projectId, roleId, dto);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating role {roleId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpDelete("DeleteRole/{roleId}")]
        public async Task<IActionResult> DeleteRole(long projectId, long roleId)
        {
            try
            {
                await (_roleBusiness).DeleteRole(projectId, roleId);
                return Ok(new { message = $"Deleted role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting role {roleId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }


    }
}