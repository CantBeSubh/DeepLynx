using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{
    [ApiController]
    // Double check with Jason, Natalie or J2 that this is how we wanted to explicity deal with routes on method level
    [Route("projects/{projectId}")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBusiness _roleBusiness;

        public RoleController(IRoleBusiness roleBusiness)
        {
            _roleBusiness = roleBusiness;
        }

        [HttpGet("GetAllRoles")]
        public ActionResult<IEnumerable<Role>> GetAll(long projectId)
        {
            var roles = _roleBusiness.GetAllRoles(projectId);
            return Ok(new { message = "Returned all Roles", data = roles});
        }

        [HttpGet("GetRole/{roleId}")]
        public ActionResult<Role> Get(long projectId, long roleId)
        {
            try 
            {
                var role = _roleBusiness.GetRole(projectId, roleId);
                return Ok(new { message = "Returned Role Requested", data = role});
            }
            catch (Exception)
            {
                return NotFound(new { message = "Role not found"});
            }
        }

        [HttpPost("CreateNewRole")]  //CHECK
        public ActionResult<Role> Post(long projectId, [FromBody] Role role)
        {
            if (role == null)
                return BadRequest(new { message = "Role is Null"});

            role.ProjectId = projectId;
            var created = _roleBusiness.CreateNewRole(role);
            return Ok(new { message = "Role created successfully", data = created});
        }

        [HttpPut("UpdateRole/{roleId}")]
        public ActionResult<Role> Put(long projectId, long roleId, [FromBody] Role role)
        {
            try 
            {
                var updated = _roleBusiness.UpdateRole(projectId, roleId, role);
                return Ok(new { mesaage = "Role updated successfully", data = updated});
            }
            catch (Exception)
            {
                return NotFound(new { message = "Role not Found"});
            }
        }

        [HttpDelete("DeleteRole/{roleId}")]
        public ActionResult Delete(long projectId, long roleId)
        {
            var success = _roleBusiness.DeleteRole(projectId, roleId);
            if (!success)
                return NotFound(new { message = "This role was not found or was already deleted"});

            return Ok(new { message = "This role was soft deleted successfully" });
        }


    }
}