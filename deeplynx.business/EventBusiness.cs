using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer.Models;
using deeplynx.models;
using deeplynx.interfaces;
using System.Text.RegularExpressions;
using Npgsql;
using deeplynx.helpers;
using deeplynx.hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

public class EventBusiness : IEventBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ICacheBusiness _cacheBusiness;
    private readonly IHubContext<EventNotificationHub>  _hubContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EventBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    public EventBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness, IHubContext<EventNotificationHub> hubContext)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Retrieves all project events that the user is subscribed to.
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscription belongs</param>
    /// <param name="projectId">The ID of the project to which the subscription belongs</param>
    public async Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId)
    {
        
        var subscriptions = await _context.Set<Subscription>()
            .Where(s => s.UserId == userId && s.ProjectId == projectId)
            .ToListAsync();

        if (!subscriptions.Any())
        {
            return new List<EventResponseDto>();
        }

        string sql = @"
            SELECT DISTINCT e.*
            FROM deeplynx.Events e
            INNER JOIN deeplynx.Subscriptions s
            ON s.project_id = e.project_id
            AND s.user_id = @userId
            AND s.project_id = @projectId
            WHERE e.project_id = @projectId
            AND ((s.entity_id = e.entity_id) OR s.entity_id IS NULL)
            AND ((s.entity_type = e.entity_type) OR s.entity_type IS NULL)
            AND ((s.data_source_id = e.data_source_id) OR s.data_source_id IS NULL)
            AND ((s.operation = e.operation) OR s.operation IS NULL)";

        var userIdParam = new NpgsqlParameter("userId", userId);
        var projectIdParam = new NpgsqlParameter("projectId", projectId);

        var events = await _context.Events.FromSqlRaw(sql, userIdParam, projectIdParam).ToListAsync();

        return events
            .Select(e => new EventResponseDto()
            {
                Id = e.Id,
                ProjectId = e.ProjectId,
                Operation = e.Operation,
                DataSourceId = e.DataSourceId,
                EntityId = e.EntityId,
                EntityType = e.EntityType,
                Properties = e.Properties,
                LastUpdatedBy = e.LastUpdatedBy,
                LastUpdatedAt = e.LastUpdatedAt,
            }).ToList();
    }
    
    /// <summary>
    /// Creates a new Event based on the event data provided.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new event to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto)
    {
        // TODO: since project may be absent, determine if this check is still needed here
        // await ExistenceHelper.EnsureProjectExistsAsync(_context, dto.ProjectId, _cacheBusiness, false);
        ValidationHelper.ValidateModel(dto);
        ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(dto.Operation, "Operation");
    
        var newEvent = new Event
        {
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            ProjectId = dto.ProjectId,
            Properties = dto.Properties,
            LastUpdatedBy = dto.LastUpdatedBy,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            DataSourceId = dto.DataSourceId,
            EntityId = dto.EntityId,
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
    
        var response = new EventResponseDto
        {
            Id = newEvent.Id,
            ProjectId = newEvent.ProjectId,
            Operation = newEvent.Operation,
            EntityType = newEvent.EntityType,
            EntityId = newEvent.EntityId,
            DataSourceId = newEvent.DataSourceId,
            Properties = newEvent.Properties,
            LastUpdatedBy = newEvent.LastUpdatedBy,
            LastUpdatedAt = newEvent.LastUpdatedAt,
        };

        // Serialize the response object to JSON using fully qualified JsonSerializer
        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);

        // Send notification with the serialized JSON object
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", jsonResponse);

        return response;
    }

    /// <summary>
    /// Bulk creates Events based on the event data provided.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the event belongs</param>
    /// <param name="events">A List of data transfer objects with details on the new event to be created.</param>
    /// <returns>The list of new Events which were created.</returns>
    public async Task<List<EventResponseDto>> BulkCreateEvents(long projectId, List<CreateEventRequestDto> events)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, false);

        foreach (var dto in events)
        {
            ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
            ValidationHelper.ValidateTypes(dto.Operation, "Operation");
        }
        var eventEntities = events.Select(dto => new Event
        {
            ProjectId = projectId,
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Properties = dto.Properties,
            DataSourceId = dto.DataSourceId,
            LastUpdatedBy = dto.LastUpdatedBy,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        }).ToList();

        _context.Events.AddRange(eventEntities);
        await _context.SaveChangesAsync();

        return eventEntities.Select(e => new EventResponseDto
        {
            Id = e.Id,
            ProjectId = e.ProjectId,
            Operation = e.Operation,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            Properties = e.Properties,
            DataSourceId = e.DataSourceId,
            LastUpdatedBy = e.LastUpdatedBy,
            LastUpdatedAt = e.LastUpdatedAt
        }).ToList();
        
    }
}