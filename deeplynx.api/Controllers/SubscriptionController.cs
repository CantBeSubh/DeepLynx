// using deeplynx.helpers.Context;
// using Microsoft.AspNetCore.Mvc;
// using deeplynx.interfaces;
// using deeplynx.models;
// using Microsoft.AspNetCore.Authorization;
//
// namespace deeplynx.api.Controllers
// {
//     /// <summary>
//     ///     Controller for managing subscriptions.
//     /// </summary>
//     /// <remarks>
//     ///     This controller provides endpoints to create, update, delete, and retrieve subscription information.
//     /// </remarks>
//     [ApiController]
//     [Route("organizations/{organizationId}/projects/{projectId}/subscriptions")]
//     [Authorize]
//     public class SubscriptionController : ControllerBase
//     {
//         private readonly ISubscriptionBusiness _subscriptionBusiness;
//         private readonly ILogger<SubscriptionController> _logger;
//
//         /// <summary>
//         ///     Initializes a new instance of the <see cref="SubscriptionController"/> class
//         /// </summary>
//         /// <param name="subscriptionBusiness">The business logic interface for handling Subscription operations.</param>
//         /// <param name="logger">Error/Info logging interface for database log table.</param>
//         // TODO: uncomment once actions can be created via API (no way to test subscriptions without an action ID)
//         public SubscriptionController(ISubscriptionBusiness subscriptionBusiness,
//             ILogger<SubscriptionController> logger)
//         {
//             _subscriptionBusiness = subscriptionBusiness;
//             _logger = logger;
//         }
//
//         /// <summary>
//         ///     Get All Subscriptions
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscriptions belong</param>
//         /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
//         /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result (Default true)</param>
//         /// <returns>A list of subscriptions for the current user in the given project.</returns>
//         [HttpGet(Name = "api_get_all_subscriptions")]
//         public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetAllSubscriptions(
//             long organizationId,
//             long projectId,
//             [FromQuery] bool hideArchived = true)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//                 var subscriptions = await _subscriptionBusiness.GetAllSubscriptions(
//                     userId, projectId, hideArchived);
//                 return Ok(subscriptions);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while listing subscriptions: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//
//         /// <summary>
//         ///     Get a Subscription
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscription belongs</param>
//         /// <param name="projectId">The ID of the project to which the subscription belongs</param>
//         /// <param name="subscriptionId">The ID of the subscription to retrieve</param>
//         /// <param name="hideArchived">Flag indicating whether to hide archived subscriptions from the result (Default true)</param>
//         /// <returns>The subscription associated with the given ID</returns>
//         [HttpGet("{subscriptionId}", Name = "api_get_subscription")]
//         public async Task<ActionResult<SubscriptionResponseDto>> GetSubscription(
//             long organizationId,
//             long projectId,
//             long subscriptionId,
//             [FromQuery] bool hideArchived = true)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//                 var subscription = await _subscriptionBusiness.GetSubscription(
//                     userId, projectId, subscriptionId, hideArchived);
//                 return Ok(subscription);
//             }
//             catch (KeyNotFoundException knfEx)
//             {
//                 var message = $"Subscription {subscriptionId} not found: {knfEx.Message}";
//                 _logger.LogWarning(message);
//                 return NotFound(message);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while retrieving subscription {subscriptionId}: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//
//         /// <summary>
//         ///     Bulk Create Subscriptions
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscriptions belong</param>
//         /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
//         /// <param name="subscriptions">List of data transfer objects containing subscription details</param>
//         /// <returns>The list of created subscriptions</returns>
//         [HttpPost("bulk", Name = "api_bulk_create_subscriptions")]
//         public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> BulkCreateSubscriptions(
//             long organizationId,
//             long projectId,
//             [FromBody] List<CreateSubscriptionRequestDto> subscriptions)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//
//                 // Ensure projectId from route is used for all subscriptions
//                 foreach (var subscription in subscriptions)
//                 {
//                     subscription.ProjectId = projectId;
//                     subscription.UserId = userId;
//                 }
//
//                 var newSubscriptions = await _subscriptionBusiness.BulkCreateSubscriptions(
//                     userId, projectId, subscriptions);
//                 return Ok(newSubscriptions);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while bulk creating subscriptions: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//
//         /// <summary>
//         ///     Bulk Update Subscriptions
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscriptions belong</param>
//         /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
//         /// <param name="subscriptions">List of data transfer objects containing updated subscription details</param>
//         /// <returns>The list of updated subscriptions</returns>
//         [HttpPut("bulk", Name = "api_bulk_update_subscriptions")]
//         public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> BulkUpdateSubscriptions(
//             long organizationId,
//             long projectId,
//             [FromBody] List<UpdateSubscriptionRequestDto> subscriptions)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//                 var updatedSubscriptions = await _subscriptionBusiness.BulkUpdateSubscriptions(
//                     userId, projectId, subscriptions);
//                 return Ok(updatedSubscriptions);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while bulk updating subscriptions: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//
//         /// <summary>
//         ///     Bulk Delete Subscriptions
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscriptions belong</param>
//         /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
//         /// <param name="subscriptionIds">List of subscription IDs to delete</param>
//         /// <returns>A message stating the subscriptions were successfully deleted.</returns>
//         [HttpDelete("bulk", Name = "api_bulk_delete_subscriptions")]
//         public async Task<IActionResult> BulkDeleteSubscriptions(
//             long organizationId,
//             long projectId,
//             [FromBody] List<long> subscriptionIds)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//                 await _subscriptionBusiness.BulkDeleteSubscriptions(userId, projectId, subscriptionIds);
//                 return Ok(new { message = $"Deleted {subscriptionIds.Count} subscriptions" });
//             }
//             catch (InvalidOperationException ioEx)
//             {
//                 var message = $"Error deleting subscriptions: {ioEx.Message}";
//                 _logger.LogWarning(message);
//                 return BadRequest(message);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while deleting subscriptions: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//
//         /// <summary>
//         ///     Bulk Archive Subscriptions
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscriptions belong</param>
//         /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
//         /// <param name="subscriptionIds">List of subscription IDs to archive</param>
//         /// <returns>A message stating the subscriptions were successfully archived.</returns>
//         [HttpPatch("bulk/archive", Name = "api_bulk_archive_subscriptions")]
//         public async Task<IActionResult> BulkArchiveSubscriptions(
//             long organizationId,
//             long projectId,
//             [FromBody] List<long> subscriptionIds)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//                 await _subscriptionBusiness.BulkArchiveSubscriptions(userId, projectId, subscriptionIds);
//                 return Ok(new { message = $"Archived {subscriptionIds.Count} subscriptions" });
//             }
//             catch (InvalidOperationException ioEx)
//             {
//                 var message = $"Error archiving subscriptions: {ioEx.Message}";
//                 _logger.LogWarning(message);
//                 return BadRequest(message);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while archiving subscriptions: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//
//         /// <summary>
//         ///     Bulk Unarchive Subscriptions
//         /// </summary>
//         /// <param name="organizationId">The ID of the organization to which the subscriptions belong</param>
//         /// <param name="projectId">The ID of the project to which the subscriptions belong</param>
//         /// <param name="subscriptionIds">List of subscription IDs to unarchive</param>
//         /// <returns>A message stating the subscriptions were successfully unarchived.</returns>
//         [HttpPatch("bulk/unarchive", Name = "api_bulk_unarchive_subscriptions")]
//         public async Task<IActionResult> BulkUnarchiveSubscriptions(
//             long organizationId,
//             long projectId,
//             [FromBody] List<long> subscriptionIds)
//         {
//             try
//             {
//                 var userId = UserContextStorage.UserId;
//                 await _subscriptionBusiness.BulkUnarchiveSubscriptions(userId, projectId, subscriptionIds);
//                 return Ok(new { message = $"Unarchived {subscriptionIds.Count} subscriptions" });
//             }
//             catch (InvalidOperationException ioEx)
//             {
//                 var message = $"Error unarchiving subscriptions: {ioEx.Message}";
//                 _logger.LogWarning(message);
//                 return BadRequest(message);
//             }
//             catch (Exception exc)
//             {
//                 var message = $"An error occurred while unarchiving subscriptions: {exc}";
//                 _logger.LogError(message);
//                 return StatusCode(StatusCodes.Status500InternalServerError, message);
//             }
//         }
//     }
// }