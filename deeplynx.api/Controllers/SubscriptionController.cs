using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    /// <summary>
    /// Controller for managing subscriptions.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to create, update, delete, and retrieve subscription information.
    /// </remarks>
    [ApiController]
    [Route("projects/{projectId}/subscriptions")]
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

        /// <summary>
        /// Get all subscriptions
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscription belongs</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result</param>
        /// <returns>List of subscription response DTOs</returns>
        [HttpGet("GetAllSubscriptions", Name = "api_get_all_subscriptions")]
        public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetAllSubscriptions(
            long projectId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var subscriptions = await _subscriptionBusiness.GetAllSubscriptions(currentUserId, projectId, hideArchived);
                return Ok(subscriptions);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get a subscription
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscription belongs</param>
        /// <param name="subscriptionId">The ID of the subscription to retrieve</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result</param>
        /// <returns>Subscription response DTO</returns>
        [HttpGet("GetSubscription/{subscriptionId}", Name = "api_get_a_subscription")]
        public async Task<ActionResult<SubscriptionResponseDto>> GetSubscription(
            long projectId, long subscriptionId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var subscription = await _subscriptionBusiness.GetSubscription(currentUserId, projectId, subscriptionId, hideArchived);
                return Ok(subscription);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching this subscription {subscriptionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create many subscriptions
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
        /// <param name="subscriptions">List of request DTOs for subscriptions</param>
        /// <returns>Bulk subscription response DTOs</returns>
        [HttpPost("BulkCreateSubscriptions", Name = "api_create_many_subscriptions")]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> BulkCreateSubscriptions(
            long projectId, [FromBody] List<CreateSubscriptionRequestDto> subscriptions)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var newSubscriptions = await _subscriptionBusiness.BulkCreateSubscriptions(currentUserId, projectId, subscriptions);
                return Ok(newSubscriptions);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating these subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update many subscriptions
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
        /// <param name="subscriptions">List of request DTOs for subscriptions</param>
        /// <returns>Bulk subscription response DTOs</returns>
        [HttpPut("BulkUpdateSubscriptions", Name = "api_update_many_subscriptions")]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> BulkUpdateSubscriptions(
            long projectId, [FromBody] List<UpdateSubscriptionRequestDto> subscriptions)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var updatedSubscriptions = await _subscriptionBusiness.BulkUpdateSubscriptions(currentUserId, projectId, subscriptions);
                return Ok(updatedSubscriptions);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating these subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete many subscriptions
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
        /// <param name="subscriptionIds">List of subscription IDs to delete</param>
        /// <returns>A message stating the subscriptions were successfully deleted.</returns>
        [HttpDelete("BulkDeleteSubscriptions", Name = "api_delete_many_subscriptions")]
        public async Task<IActionResult> BulkDeleteSubscriptions(long projectId, [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkDeleteSubscriptions(currentUserId, projectId, subscriptionIds);
                return Ok(new { message = $"Deleted subscriptions" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive many subscriptions
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
        /// <param name="subscriptionIds">List of subscription IDs to archive</param>
        /// <returns>A message stating the subscriptions were successfully archived.</returns>
        [HttpPut("BulkArchiveSubscriptions", Name = "api_archive_many_subscriptions")]
        public async Task<IActionResult> BulkArchiveSubscriptions(long projectId, [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkArchiveSubscriptions(currentUserId, projectId, subscriptionIds);
                return Ok(new { message = $"Archived subscriptions" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive many subscriptions
        /// </summary>
        /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
        /// <param name="subscriptionIds">List of subscription IDs to unarchive</param>
        /// <returns>A message stating the subscriptions were successfully unarchived.</returns>
        [HttpPut("BulkUnarchiveSubscriptions", Name = "api_unarchive_many_subscriptions")]
        public async Task<IActionResult> BulkUnarchiveSubscriptions(long projectId, [FromBody] List<long> subscriptionIds)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _subscriptionBusiness.BulkUnarchiveSubscriptions(currentUserId, projectId, subscriptionIds);
                return Ok(new { message = $"Unarchived subscriptions" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving subscriptions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}
