using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/user")]
    [NexusAuthorize]
    public class UserController : ControllerBase
    {
        private readonly IUserBusiness _userBusiness;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class
        /// </summary>
        /// <param name="userBusiness">The business logic interface for handling user operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public UserController(IUserBusiness userBusiness, ILogger<UserController> logger)
        {
            _userBusiness = userBusiness;
            _logger = logger;
        }
        
        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="projectId">(Optional) ID of project that users are associated with</param>
        /// <param name="organizationId">(Optional) ID of organization that users are associated with</param>
        /// <returns>List of user response DTOs</returns>
        [HttpGet("GetAllUsers", Name = "api_get_all_users")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers(
            [FromQuery] long? projectId, 
            [FromQuery] long? organizationId)
        {
            try
            {
                var users = await _userBusiness.GetAllUsers(projectId, organizationId);
                return Ok(users);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all users.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }
        /// <summary>
        /// Get a user
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>User response DTO</returns>
        [HttpGet("GetUser/{userId}", Name = "api_get_a_user")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        
        }
        
        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="dto">User request DTO</param>
        /// <returns>User response DTO</returns>
        [HttpPost("CreateUser", Name = "api_create_a_user")]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserRequestDto dto)
        {
            try
            {
                var newUser = await _userBusiness.CreateUser(dto);
                return Ok(newUser);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating this user.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Refresh stored user info
        /// </summary>
        /// <param name="dto">User request DTO</param>
        /// <returns>User response DTO</returns>
        [HttpPost("RefreshUser", Name = "api_refresh_user")]
        public async Task<ActionResult<UserResponseDto>> RefreshUser([FromBody] CreateUserRequestDto dto)
        {
            try
            {
                var user = await _userBusiness.RefreshUser(dto);
                return Ok(user);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating or updating this user: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Update a user
        /// </summary>
        /// /// <param name="userId">ID of user</param>
        /// <param name="dto">User request DTO</param>
        /// <returns>User response DTO</returns>
        [HttpPut("UpdateUser/{userId}", Name = "api_update_a_user")]
        public async Task<ActionResult<UserResponseDto>> UpdateClass(long userId, [FromBody] UpdateUserRequestDto dto)
        {
            try
            {
                var updatedUser = await _userBusiness.UpdateUser(userId, dto);
                return Ok(updatedUser);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating this user {userId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Deletes a user 
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>A message stating the user was successfully deleted.</returns>
        [HttpDelete("DeleteUser/{userId}", Name = "api_delete_a_user")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a user 
        /// </summary>
        /// <param name="userId">The ID of the user to archive.</param>
        /// <returns>A message stating the user was successfully archived.</returns>
        [HttpDelete("ArchiveUser/{userId}", Name = "api_archive_a_user")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get data overview for user
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>Data overview DTO</returns>
        [HttpGet("GetDataOverview/{userId}", Name = "api_get_a_user_overview")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        
        }
        
        /// <summary>
        /// Unarchive a user 
        /// </summary>
        /// <param name="userId">The ID of the user to unarchive.</param>
        /// <returns>A message stating the user was successfully unarchived.</returns>
        [HttpPut("UnarchiveUser/{userId}", Name = "api_unarchive_a_user")]
        public async Task<IActionResult> UnarchiveUser(long userId)
        {
            try
            {
                await _userBusiness.UnarchiveUser(userId);
                return Ok(new { message = $"Unarchived user {userId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving user {userId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get recent records
        /// </summary>
        /// <param name="projectId">Array of project ids</param>
        /// <returns>List of record response DTOs sorted by most recent</returns>
        [HttpGet("GetRecentlyAddedRecords", Name = "api_get_recent_records")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> GetRecentlyAddedRecords([FromQuery] long[] projectId)
        {
            try
            {
                var records = await _userBusiness.GetRecentlyAddedRecords(projectId);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing historical records: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}