using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

/// <summary>
///     Controller for managing events.
/// </summary>
/// <remarks>
///     This controller provides endpoints to create, retrieve, and query event information.
/// </remarks>
[ApiController]
[Route("organizations/{organizationId}/projects/{projectId}/events")]
[Authorize]
public class EventController : ControllerBase
{
    private readonly IEventBusiness _eventBusiness;
    private readonly ILogger<EventController> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventController"/> class
    /// </summary>
    /// <param name="eventBusiness">The business logic interface for handling Event operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public EventController(IEventBusiness eventBusiness, ILogger<EventController> logger)
    {
        _eventBusiness = eventBusiness;
        _logger = logger;
    }

    /// <summary>
    ///     Get All Events
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the events belong</param>
    /// <param name="projectId">The ID of the project to which the events belong</param>
    /// <returns>A list of events for the given organization/project.</returns>
    [HttpGet(Name = "api_get_all_events")]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEvents(
        long organizationId,
        long projectId)
    {
        try
        {
            var events = await _eventBusiness.GetAllEvents(projectId, organizationId);
            return Ok(events);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while listing events: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Query Events with Pagination
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the events belong</param>
    /// <param name="projectId">The ID of the project to which the events belong</param>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <returns>Paginated response containing events and metadata</returns>
    [HttpGet("query", Name = "api_query_events")]
    public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEvents(
        long organizationId,
        long projectId,
        [FromQuery] EventsQueryRequestDTO? queryDto)
    {
        try
        {
            var events = await _eventBusiness.QueryEvents(
                projectId, organizationId, queryDto);
            return Ok(events);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while querying events: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Query Events by User Project Membership
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the events belong</param>
    /// <param name="projectId">The ID of the project to which the events belong</param>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <returns>Paginated response containing events for projects the user is a member of</returns>
    [HttpGet("by-user", Name = "api_query_events_by_user")]
    public async Task<ActionResult<PaginatedResponse<EventResponseDto>>> QueryEventsByUser(
        long projectId,
        long organizationId,
        [FromQuery] EventsQueryRequestDTO? queryDto)
    {
        try
        {
            var userId = UserContextStorage.UserId;
            var events = await _eventBusiness.QueryEventsByUser(
                userId, projectId, organizationId, queryDto);
            return Ok(events);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while querying events by user: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get Events by User Project Subscriptions
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the events belong</param>
    /// <param name="projectId">The ID of the project to which the events belong</param>
    /// <param name="userId">The ID of the user whose subscriptions to filter by</param>
    /// <returns>A list of events matching the user's subscriptions</returns>
    [HttpGet("subscriptions", Name = "api_get_events_by_subscriptions")]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEventsByUserSubscriptions(
        long organizationId,
        long projectId,
        [FromQuery] long userId)
    {
        try
        {
            var events = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(userId, projectId);
            return Ok(events);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while fetching events by subscriptions: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Create an Event
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the event belongs</param>
    /// <param name="projectId">The ID of the project to which the event belongs</param>
    /// <param name="dto">The data transfer object containing event details</param>
    /// <returns>The created event</returns>
    [HttpPost(Name = "api_create_event")]
    public async Task<ActionResult<EventResponseDto>> CreateEvent(
        long organizationId,
        long projectId,
        [FromBody] CreateEventRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;

            // Ensure IDs from routes are used
            dto.ProjectId = projectId;
            dto.OrganizationId = organizationId;

            var eventResponse = await _eventBusiness.CreateEvent(
                currentUserId, dto, projectId, organizationId);
            return Ok(eventResponse);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while creating event: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Bulk Create Events
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the events belong</param>
    /// <param name="projectId">The ID of the project to which the events belong</param>
    /// <param name="events">List of data transfer objects containing event details</param>
    /// <returns>The list of created events</returns>
    [HttpPost("bulk", Name = "api_bulk_create_events")]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> BulkCreateEvents(
        long organizationId,
        long projectId,
        [FromBody] List<CreateEventRequestDto> events)
    {
        try
        {
            // Ensure organizationId from route is used for all events
            foreach (var evt in events)
            {
                evt.OrganizationId = organizationId;
                evt.ProjectId = projectId;
            }

            var eventResponses = await _eventBusiness.BulkCreateEvents(
                events, projectId, organizationId);
            return Ok(eventResponses);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while bulk creating events: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}