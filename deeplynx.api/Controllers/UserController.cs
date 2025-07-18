using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserBusiness _userBusiness;

        public UserController(IUserBusiness userBusiness)
        {
            _userBusiness = userBusiness;
        }
        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="projectId">(Optional) ID of project that users are associated with</param>
        /// <returns>List of user response DTOs</returns>
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers(long? projectId)
        {
            try
            {
                var users = await _userBusiness.GetAllUsers(projectId);
                return Ok(users);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all users.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }
        /// <summary>
        /// Get a user
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>User response DTO</returns>
        [HttpGet("GetUser/{userId}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(long userId)
        {
            try
            {
                var user = await _userBusiness.GetUser(userId);
                return Ok(user);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching user {userId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        
        }
        
        
        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="dto">User request DTO</param>
        /// <returns>User response DTO</returns>
        [HttpPost("CreateUser")]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserRequestDto dto)
        {
            try
            {
                var newUser = await _userBusiness.CreateUser(dto);
                return Ok(newUser);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating this user.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        /// <summary>
        /// Update a user
        /// </summary>
        /// /// <param name="userId">ID of user</param>
        /// <param name="dto">User request DTO</param>
        /// <returns>User response DTO</returns>
        [HttpPut("UpdateUser/{userId}")]
        public async Task<ActionResult<UserResponseDto>> UpdateClass(long userId, [FromBody] UserRequestDto dto)
        {
            try
            {
                var updatedUser = await _userBusiness.UpdateUser(userId, dto);
                return Ok(updatedUser);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating this user {userId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Deletes a user 
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>A message stating the user was successfully deleted.</returns>
        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(long userId)
        {
            try
            { 
                await _userBusiness.DeleteUser(userId);
                return Ok(new { message = $"Deleted user {userId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting user {userId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a user 
        /// </summary>
        /// <param name="userId">The ID of the user to archive.</param>
        /// <returns>A message stating the user was successfully archived.</returns>
        [HttpDelete("ArchiveUser/{userId}")]
        public async Task<IActionResult> ArchiveUser(long userId)
        {
            try
            {
                await _userBusiness.ArchiveUser(userId);
                return Ok(new { message = $"Archived user {userId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving user {userId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get data overview for user
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>Data overview DTO</returns>
        [HttpGet("GetDataOverview/{userId}")]
        public async Task<ActionResult<DataOverviewDto>> GetDataOverview(long userId)
        {
            try
            {
                var user = await _userBusiness.GetUserOverview(userId);
                return Ok(user);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching user {userId} data overview: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        
        }
        
        /// <summary>
        /// Add user to project
        /// </summary>
        /// <param name="userId">Id of user to be added</param>
        /// /// <param name="projectId">Id of project to add user to</param>
        /// <returns>Success message</returns>
        [HttpPost("AddUserToProject")]
        public async Task<IActionResult> AddUserToProject(long userId, long projectId)
        {
            try
            { 
                await _userBusiness.AddUserToProject(userId, projectId);
                return Ok(new { message = $"Added user {userId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while adding user {userId} to project {projectId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Remove user from project
        /// </summary>
        /// <param name="userId">Id of user to be removed</param>
        /// /// <param name="projectId">Id of project to remove user from</param>
        /// <returns>Success message</returns>
        [HttpPost("RemoveUserFromProject")]
        public async Task<IActionResult> RemoveUserFromProject(long userId, long projectId)
        {
            try
            { 
                await _userBusiness.RemoveUserFromProject(userId, projectId);
                return Ok(new { message = $"Removed user {userId} from project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while removing user {userId} from project {projectId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}