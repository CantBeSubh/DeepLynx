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
        /// <returns></returns>
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(long? projectId)
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
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("GetUser/{userId}")]
        public async Task<IActionResult> GetUser(long userId)
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
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserRequestDto dto)
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
        /// /// <param name="userId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("UpdateUser/{userId}")]
        public async Task<IActionResult> UpdateClass(long userId, [FromBody] UserRequestDto dto)
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
    }
}