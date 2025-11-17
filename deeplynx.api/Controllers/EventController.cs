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
        
        // ==================== Organization-level endpoints ====================
        
        /// <summary>
        /// Get All Events for an Organization (Admin only)
        /// </summary>
        /// <param name="organizationId">Required organization ID to filter events</param>
        [HttpGet("organizations/{organizationId}", Name = "api_get_all_events_org")]
        public async Task<ActionResult<List<EventResponseDto>>> GetAllEventsForOrganization(
            [FromRoute] long organizationId
        )
        {
            try
            {
                var events = await _eventBusiness.GetAllEvents(organizationId, null);
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
        /// Query Events for an Organization (Paginated, Admin only)
        /// </summary>
        /// <param name="organizationId">Required organization ID</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("organizations/{organizationId}/query", Name = "api_query_events_org")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsForOrganization(
            [FromRoute] long organizationId,
            [FromQuery] EventsQueryRequestDTO? queryDto
        )
        {
            try
            {
                var events = await _eventBusiness.QueryEvents(organizationId, queryDto, null);
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
        /// <param name="userId">The ID of the user</param>
        [HttpGet("organizations/{organizationId}/users/{userId}/subscriptions", Name = "api_get_events_by_user_subscriptions_org")]
        public async Task<ActionResult<List<EventResponseDto>>> GetEventsByUserSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromRoute] long userId)
        {
            try
            {
                var events = await _eventBusiness.GetAllEventsByUserSubscriptions(organizationId, userId, null);
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
        /// <param name="userId">The ID of the user</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("organizations/{organizationId}/users/{userId}/subscriptions/query", Name = "api_query_events_by_user_subscriptions_org")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUserSubscriptionsForOrganization(
            [FromRoute] long organizationId,
            [FromRoute] long userId,
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var events = await _eventBusiness.QueryEventsByUserSubscriptions(organizationId, userId, queryDto, null);
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
        /// Get All Events for a Project
        /// </summary>
        /// <param name="organizationId">Required organization ID</param>
        /// <param name="projectId">Required project ID to filter events</param>
        [HttpGet("organizations/{organizationId}/projects/{projectId}", Name = "api_get_all_events_project")]
        public async Task<ActionResult<List<EventResponseDto>>> GetAllEventsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId
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
        /// Query Events for a Project (Paginated)
        /// </summary>
        /// <param name="organizationId">Required organization ID</param>
        /// <param name="projectId">Required project ID</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("organizations/{organizationId}/projects/{projectId}/query", Name = "api_query_events_project")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long projectId,
            [FromQuery] EventsQueryRequestDTO? queryDto
        )
        {
            try
            {
                var events = await _eventBusiness.QueryEvents(organizationId, queryDto, projectId);
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
        /// <param name="userId">The ID of the user</param>
        /// <param name="projectId">Required project ID</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("organizations/{organizationId}/projects/{projectId}/users/{userId}", Name = "api_query_events_by_user_project")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUserForProject(
            [FromRoute] long organizationId,
            [FromRoute] long userId,
            [FromRoute] long projectId,
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var events = await _eventBusiness.QueryEventsByUser(organizationId, userId, queryDto, projectId);
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
        /// <param name="userId">The ID of the user</param>
        /// <param name="projectId">The ID of the project</param>
        [HttpGet("organizations/{organizationId}/projects/{projectId}/users/{userId}/subscriptions", Name = "api_get_events_by_user_subscriptions_project")]
        public async Task<ActionResult<List<EventResponseDto>>> GetEventsByUserSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long userId,
            [FromRoute] long projectId)
        {
            try
            {
                var events = await _eventBusiness.GetAllEventsByUserSubscriptions(organizationId, userId, projectId);
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
        /// <param name="userId">The ID of the user</param>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        [HttpGet("organizations/{organizationId}/projects/{projectId}/users/{userId}/subscriptions/query", Name = "api_query_events_by_user_subscriptions_project")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUserSubscriptionsForProject(
            [FromRoute] long organizationId,
            [FromRoute] long userId,
            [FromRoute] long projectId,
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var events = await _eventBusiness.QueryEventsByUserSubscriptions(organizationId, userId, queryDto, projectId);
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