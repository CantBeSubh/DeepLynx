using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    /// <summary>
    /// Controller for fetching events that match user subscriptions.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to retrieve events at organization and project levels.
    /// </remarks>

    [ApiController]
    [Route("events")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventBusiness _eventBusiness;
        private readonly ILogger<EventController> _logger;
        
        public EventController(IEventBusiness eventBusiness, ILogger<EventController> logger)
        {
            _eventBusiness = eventBusiness;
            _logger = logger;
        }
        
        // ==================== Site-level endpoints ====================
        /// <summary>
        /// Get All Events (Site Admin only)
        /// </summary>
        /// <param name="organizationId">Required organization ID to filter events</param>
        /// <param name="projectId">Optional project ID to filter events</param>
        [HttpGet("GetAllEvents", Name = "api_get_all_events_site")]
        public async Task<ActionResult<List<EventResponseDto>>> GetAllEvents(
            [FromRoute] long organizationId,
            [FromQuery] long? projectId
        )
        {
            try
            {
                var events = await _eventBusiness.GetAllEvents(organizationId, projectId);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Query All Events (Site Admin only)
        /// </summary>
        /// <param name="organizationId">Required organization ID to filter events</param>
        /// <param name="queryDto">Optional Data Transfer Object with filter options</param>
        /// <param name="projectId">Optional project ID to filter events</param>
        [HttpGet("QueryAllEvents", Name = "api_query_all_events_site")]
        public async Task<ActionResult<List<EventResponseDto>>> QueryAllEvents(
            [FromRoute] long organizationId,
            [FromQuery] EventsQueryRequestDTO? queryDto,
            [FromQuery] long? projectId
        )
        {
            try
            {
                var events = await _eventBusiness.QueryAllEvents(organizationId, queryDto, projectId);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        // ==================== Organization-level endpoints ====================
        
        /// <summary>
        /// Query All Events for an Organization (Paginated, Admin only)
        /// </summary>
        /// <param name="organizationId">Required organization ID to filter events</param>
        [HttpGet("QueryAuthorizedEventByOrg", Name = "api_query_authorized_events_org")]
        public async Task<ActionResult<List<EventResponseDto>>> QueryAuthorizedEventsForOrganization(
            [FromRoute] long organizationId,
            [FromQuery] EventsQueryRequestDTO? queryDto
        )
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.QueryAuthorizedEvents(currentUserId, organizationId, queryDto, null);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get events by user subscriptions at organization level.
        /// Returns all organization-level events that match the user's subscriptions.
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        [HttpGet("GetEventsBySubscriptionsByOrg", Name = "api_get_events_by_subscriptions_org")]
        public async Task<ActionResult<List<EventResponseDto>>> GetEventsBySubscriptionsForOrganization(
            [FromRoute] long organizationId)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.GetAllEventsBySubscriptions(currentUserId, organizationId, null);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Query events by user subscriptions at organization level (Paginated).
        /// Returns all organization-level events that match the user's subscriptions with filtering.
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("QueryEventsBySubscriptionsByOrg", Name = "api_query_events_by_user_subscriptions_org")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUserSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.QueryEventsBySubscriptions(currentUserId, organizationId, queryDto, null);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        // ==================== Project-level endpoints ====================

        /// <summary>
        /// Query Events for a Project (Paginated)
        /// </summary>
        /// <param name="organizationId">Required organization ID</param>
        /// <param name="projectId">Required project ID</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("QueryAllEventsByProject/{projectId}", Name = "api_query_events_project")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromQuery] EventsQueryRequestDTO? queryDto
        )
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.QueryAuthorizedEvents(currentUserId, organizationId, queryDto, projectId);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Query events by user project membership (Paginated).
        /// Returns events from projects the user is a member of.
        /// </summary>
        /// <param name="organizationId">Required organization ID</param>
        /// <param name="projectId">Required project ID</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("QueryAuthorizedEventsByProject/{projectId}", Name = "api_query_authorized_events_project")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUserForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.QueryAuthorizedEvents(currentUserId, organizationId, queryDto, projectId);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get events by user subscriptions at project level.
        /// Returns project-specific events that match the user's subscriptions.
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        [HttpGet("GetEventsByUserSubscriptionsByProject/{projectId}", Name = "api_get_events_by_subscriptions_project")]
        public async Task<ActionResult<List<EventResponseDto>>> GetEventsByUserSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.GetAllEventsBySubscriptions(currentUserId, organizationId, projectId);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Query events by user subscriptions at project level (Paginated).
        /// Returns project-specific events that match the user's subscriptions with filtering.
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("QueryEventsBySubscriptionsByProject/{projectId}", Name = "api_query_events_by_user_subscriptions_project")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUserSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var events = await _eventBusiness.QueryEventsBySubscriptions(currentUserId, organizationId, queryDto, projectId);
                return Ok(events);
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred while fetching events: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}