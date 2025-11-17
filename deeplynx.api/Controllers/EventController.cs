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
    /// This controller provides an endpoint retrieve project events that match the user's subscriptions.
    /// </remarks>

    [ApiController]
    [Route("events")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventBusiness _eventBusiness;
        private readonly ILogger<EventController> _logger;
        public EventController(IEventBusiness eventBusiness,  ILogger<EventController> logger)
        {
            _eventBusiness = eventBusiness;
            _logger = logger;
        }
        
        /// <summary>
        /// Get All Events
        /// </summary>
        /// <param name="organizationId">Required organization ID to filter events</param>
        /// <param name="projectId">Optional project ID to further filter events</param>
        /// <returns></returns>
        [HttpGet("GetAllEvents", Name = "api_get_all_events")]
        public async Task<ActionResult<List<EventResponseDto>>> GetAllEvents(
            [FromQuery] long organizationId,
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
        /// Get All Events (Paginated)
        /// </summary>
        /// <param name="queryDto">Filter criteria and pagination parameters</param>
        /// <returns></returns>
        [HttpGet("QueryEvents", Name = "api_query_events_paginated")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEvents(
            [FromQuery] EventsQueryRequestDTO? queryDto
        )
        {
            try
            {
                var events = await _eventBusiness.QueryEvents(queryDto);
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
        /// Get all events by user project membership (Paginated).
        /// </summary>
        [HttpGet("QueryEventsByUser", Name = "api_query_events_by_user")]
        public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUser(
            [FromQuery] EventsQueryRequestDTO? queryDto)
        {
            try
            {
                var events = await _eventBusiness.QueryEventsByUser(queryDto);
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
        /// Get events by user subscriptions at organization and/or project level.
        /// Organization-level subscriptions return all events in the organization.
        /// Project-level subscriptions return only events in that specific project.
        /// </summary>
        /// <param name="organizationId">The ID of the organization</param>
        /// <param name="userId">The ID of the user</param>
        /// <param name="projectId">Optional project ID to filter events to a specific project</param>
        [HttpGet("GetEventsByUserSubscriptions", Name = "api_get_events_by_user_subscriptions")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEventsByUserSubscriptions(
            [FromQuery] long organizationId,
            [FromQuery] long userId,
            [FromQuery] long? projectId)
        {
            try
            {
                var events = await _eventBusiness.GetAllEventsByUserSubscriptions(userId, organizationId, projectId);
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
