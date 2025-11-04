using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.helpers;
using deeplynx.helpers.Context;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("oauth-applications")]
    public class OauthApplicationController : ControllerBase
    {
        private readonly IOauthApplicationBusiness _oauthApplicationBusiness;
        private readonly ILogger<OauthApplicationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OauthApplicationController"/> class
        /// </summary>
        /// <param name="oauthApplicationBusiness">The business logic interface for handling OAuth Application operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public OauthApplicationController(
            IOauthApplicationBusiness oauthApplicationBusiness,
            ILogger<OauthApplicationController> logger)
        {
            _oauthApplicationBusiness = oauthApplicationBusiness;
            _logger = logger;
        }

        /// <summary>
        /// List all OAuth applications
        /// </summary>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived applications</param>
        /// <returns></returns>
        [HttpGet("GetAllOauthApplications", Name = "api_get_all_oauth_applications")]
        public async Task<ActionResult<IEnumerable<OauthApplicationResponseDto>>> GetAllOauthApplications(
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var applications = await _oauthApplicationBusiness
                    .GetAllOauthApplications(hideArchived);
                return Ok(applications);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing OAuth applications: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Fetch OAuth Application by ID
        /// </summary>
        /// <param name="applicationId">ID of OAuth application</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived applications</param>
        /// <returns></returns>
        [HttpGet("GetOauthApplication/{applicationId}", Name = "api_get_oauth_application")]
        public async Task<ActionResult<OauthApplicationResponseDto>> GetOauthApplication(
            long applicationId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var application = await _oauthApplicationBusiness.GetOauthApplication(applicationId, hideArchived);
                return Ok(application);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving OAuth application {applicationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create an OAuth Application
        /// </summary>
        /// <param name="dto">Data structure of OAuth application to create</param>
        /// <returns></returns>
        [HttpPost("CreateOauthApplication", Name = "api_create_oauth_application")]
        public async Task<ActionResult<OauthApplicationSecureResponseDto>> CreateOauthApplication(
            [FromBody] CreateOauthApplicationRequestDto dto)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                var application = await _oauthApplicationBusiness.CreateOauthApplication(dto, currentUserId);
                return Ok(application);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating OAuth application: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update an OAuth Application
        /// </summary>
        /// <param name="applicationId">ID of the OAuth application</param>
        /// <param name="dto">Fields to update</param>
        /// <returns></returns>
        [HttpPut("UpdateOauthApplication/{applicationId}", Name = "api_update_oauth_application")]
        public async Task<ActionResult<OauthApplicationResponseDto>> UpdateOauthApplication(
            long applicationId,
            [FromBody] UpdateOauthApplicationRequestDto dto)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                var application = await _oauthApplicationBusiness.UpdateOauthApplication(applicationId, dto, currentUserId);
                return Ok(application);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating OAuth application {applicationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete an OAuth application
        /// </summary>
        /// <param name="applicationId">ID of the OAuth application to hard delete</param>
        /// <returns></returns>
        [HttpDelete("DeleteOauthApplication/{applicationId}", Name = "api_delete_oauth_application")]
        public async Task<ActionResult> DeleteOauthApplication(long applicationId)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                await _oauthApplicationBusiness.DeleteOauthApplication(applicationId, currentUserId);
                return Ok(new { message = $"Deleted OAuth application {applicationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting OAuth application {applicationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive an OAuth application
        /// </summary>
        /// <param name="applicationId">ID of the OAuth application</param>
        /// <returns></returns>
        [HttpDelete("ArchiveOauthApplication/{applicationId}", Name = "api_archive_oauth_application")]
        public async Task<ActionResult> ArchiveOauthApplication(long applicationId)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                await _oauthApplicationBusiness.ArchiveOauthApplication(applicationId, currentUserId);
                return Ok(new { message = $"Archived OAuth application {applicationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving OAuth application {applicationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive an OAuth Application
        /// </summary>
        /// <param name="applicationId">ID of the OAuth application</param>
        /// <returns></returns>
        [HttpPut("UnarchiveOauthApplication/{applicationId}", Name = "api_unarchive_oauth_application")]
        public async Task<ActionResult> UnarchiveOauthApplication(long applicationId)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                await _oauthApplicationBusiness.UnarchiveOauthApplication(applicationId, currentUserId);
                return Ok(new { message = $"Unarchived OAuth application {applicationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving OAuth application {applicationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}