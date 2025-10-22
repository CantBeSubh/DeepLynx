using deeplynx.helpers;
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
    [Route("api/events")]
    [Authorize]
    public class EventController : ControllerBase // Inherit from ControllerBase
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
        /// <param name="projectId">The ID of the project</param>
        /// <param name="organizationId">The ID of the origanization to which the events belong</param>
        /// <returns></returns>
        [HttpGet("GetAllEvents", Name = "api_get_all_events")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEvents(
            [FromQuery] long? projectId = null,
            [FromQuery] long? organizationId = null)
        {
            try
            {
                var events = await _eventBusiness.GetAllEvents(projectId, organizationId);
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
        /// Get all events by user project membership.
        /// </summary>
        [HttpGet("GetAllEventsByUser", Name = "api_get_all_events_by_user")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEventsByUser()
        {
            try
            {
                var events = await _eventBusiness.GetAllEventsByUser();
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
        /// Get project Events based on user subscriptions 
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="projectId">The ID of the project to which the events belong</param>
        /// <returns></returns>

        [HttpGet("{projectId}/GetAllEventsByUserProjectSubscriptions", Name = "api_get_all_events_by_user_project_subscriptions")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEventsByUserProjectSubscriptions(
            long projectId,
            [FromQuery] long userId)
        {
            try
            {
                var events = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(projectId, userId);
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
