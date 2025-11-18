using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using deeplynx.helpers.Context;

namespace deeplynx.api.Controllers
{
    /// <summary>
    /// Controller for managing subscriptions.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to create, update, delete, and retrieve subscription information
    /// at both organization and project levels.
    /// </remarks>
    [ApiController]
    [Route("subscriptions")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionBusiness _subscriptionBusiness;
        private readonly ILogger<SubscriptionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionController"/> class.
        /// </summary>
        /// <param name="subscriptionBusiness">The business logic interface for handling subscription operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public SubscriptionController(ISubscriptionBusiness subscriptionBusiness, ILogger<SubscriptionController> logger)
        {
            _subscriptionBusiness = subscriptionBusiness;
            _logger = logger;
        }

        // ==================== Organization-level endpoints ====================

        /// <summary>
        /// Get all subscriptions for the current user at organization or project level
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">Optional project ID to scope subscriptions to a specific project</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result</param>
        /// <returns>List of subscription response DTOs</returns>
        [HttpGet("GetAllSubscriptions", Name = "api_get_all_subscriptions")]
        public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetAllSubscriptions(
            [FromRoute] long organizationId,
            [FromQuery] long? projectId = null,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var subscriptions = await _subscriptionBusiness.GetAllSubscriptions(currentUserId, organizationId, hideArchived, projectId);
                return Ok(subscriptions);
            }
            catch (Exception exc)
            {
                var scope = projectId.HasValue ? "project" : "organization";
                var message = $"An unexpected error occurred while fetching all {scope} subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get a specific organization-level subscription
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="subscriptionId">The ID of the subscription to retrieve</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result</param>
        /// <returns>Subscription response DTO</returns>
        [HttpGet("GetSubscriptionByOrg/{subscriptionId}", Name = "api_get_subscription_org")]
        public async Task<ActionResult<SubscriptionResponseDto>> GetSubscriptionForOrganization(
            [FromRoute] long organizationId,
            [FromRoute] long subscriptionId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var subscription = await _subscriptionBusiness.GetSubscription(currentUserId, subscriptionId, organizationId, hideArchived, null);
                return Ok(subscription);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Subscription with id {subscriptionId} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching subscription {subscriptionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create many organization-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="subscriptions">List of request DTOs for subscriptions</param>
        /// <returns>List of created subscription response DTOs</returns>
        [HttpPost("BulkCreateSubscriptionsByOrg", Name = "api_bulk_create_subscriptions_org")]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> BulkCreateSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromBody] List<CreateSubscriptionRequestDto> subscriptions)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var newSubscriptions = await _subscriptionBusiness.BulkCreateSubscriptions(currentUserId, organizationId, subscriptions, null);
                return Ok(newSubscriptions);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating organization subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update many organization-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="subscriptions">List of request DTOs for subscriptions</param>
        /// <returns>List of updated subscription response DTOs</returns>
        [HttpPut("BulkUpdateSubscriptionsByOrg", Name = "api_bulk_update_subscriptions_org")]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> BulkUpdateSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromBody] List<UpdateSubscriptionRequestDto> subscriptions)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var updatedSubscriptions = await _subscriptionBusiness.BulkUpdateSubscriptions(currentUserId, organizationId, subscriptions, null);
                return Ok(updatedSubscriptions);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating organization subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete many organization-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="subscriptionIds">List of subscription IDs to delete</param>
        /// <returns>A message stating the subscriptions were successfully deleted</returns>
        [HttpDelete("BulkDeleteSubscriptionsByOrg", Name = "api_bulk_delete_subscriptions_org")]
        public async Task<IActionResult> BulkDeleteSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkDeleteSubscriptions(currentUserId, organizationId, subscriptionIds, null);
                return Ok(new { message = "Deleted organization subscriptions" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting organization subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive many organization-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="subscriptionIds">List of subscription IDs to archive</param>
        /// <returns>A message stating the subscriptions were successfully archived</returns>
        [HttpPut("BulkArchiveSubscriptionsByOrg", Name = "api_bulk_archive_subscriptions_org")]
        public async Task<IActionResult> BulkArchiveSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkArchiveSubscriptions(currentUserId, organizationId, subscriptionIds, null);
                return Ok(new { message = "Archived organization subscriptions" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving organization subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive many organization-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="subscriptionIds">List of subscription IDs to unarchive</param>
        /// <returns>A message stating the subscriptions were successfully unarchived</returns>
        [HttpPut("BulkUnarchiveSubscriptionsByOrg", Name = "api_bulk_unarchive_subscriptions_org")]
        public async Task<IActionResult> BulkUnarchiveSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkUnarchiveSubscriptions(currentUserId, organizationId, subscriptionIds, null);
                return Ok(new { message = "Unarchived organization subscriptions" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving organization subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        // ==================== Project-level endpoints ====================

        /// <summary>
        /// Get a specific project-level subscription
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="subscriptionId">The ID of the subscription to retrieve</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result</param>
        /// <returns>Subscription response DTO</returns>
        [HttpGet("GetSubscriptionByProject/{projectId}/{subscriptionId}", Name = "api_get_subscription_project")]
        public async Task<ActionResult<SubscriptionResponseDto>> GetSubscriptionForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromRoute] long subscriptionId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var subscription = await _subscriptionBusiness.GetSubscription(currentUserId, subscriptionId, organizationId, hideArchived, projectId);
                return Ok(subscription);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Subscription with id {subscriptionId} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching subscription {subscriptionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create many project-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="subscriptions">List of request DTOs for subscriptions</param>
        /// <returns>List of created subscription response DTOs</returns>
        [HttpPost("BulkCreateSubscriptionsByProject/{projectId}", Name = "api_bulk_create_subscriptions_project")]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> BulkCreateSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromBody] List<CreateSubscriptionRequestDto> subscriptions)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var newSubscriptions = await _subscriptionBusiness.BulkCreateSubscriptions(currentUserId, organizationId, subscriptions, projectId);
                return Ok(newSubscriptions);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating project subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update many project-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="subscriptions">List of request DTOs for subscriptions</param>
        /// <returns>List of updated subscription response DTOs</returns>
        [HttpPut("BulkUpdateSubscriptionsByProject/{projectId}", Name = "api_bulk_update_subscriptions_project")]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> BulkUpdateSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromBody] List<UpdateSubscriptionRequestDto> subscriptions)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var updatedSubscriptions = await _subscriptionBusiness.BulkUpdateSubscriptions(currentUserId, organizationId, subscriptions, projectId);
                return Ok(updatedSubscriptions);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating project subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete many project-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="subscriptionIds">List of subscription IDs to delete</param>
        /// <returns>A message stating the subscriptions were successfully deleted</returns>
        [HttpDelete("BulkDeleteSubscriptionsByProject/{projectId}", Name = "api_bulk_delete_subscriptions_project")]
        public async Task<IActionResult> BulkDeleteSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkDeleteSubscriptions(currentUserId, organizationId, subscriptionIds, projectId);
                return Ok(new { message = "Deleted project subscriptions" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting project subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive many project-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="subscriptionIds">List of subscription IDs to archive</param>
        /// <returns>A message stating the subscriptions were successfully archived</returns>
        [HttpPut("BulkArchiveSubscriptionsByProject/{projectId}", Name = "api_bulk_archive_subscriptions_project")]
        public async Task<IActionResult> BulkArchiveSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkArchiveSubscriptions(currentUserId, organizationId, subscriptionIds, projectId);
                return Ok(new { message = "Archived project subscriptions" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving project subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive many project-level subscriptions
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="subscriptionIds">List of subscription IDs to unarchive</param>
        /// <returns>A message stating the subscriptions were successfully unarchived</returns>
        [HttpPut("BulkUnarchiveSubscriptionsByProject/{projectId}", Name = "api_bulk_unarchive_subscriptions_project")]
        public async Task<IActionResult> BulkUnarchiveSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkUnarchiveSubscriptions(currentUserId, organizationId, subscriptionIds, projectId);
                return Ok(new { message = "Unarchived project subscriptions" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving project subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}