using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer.Models;
using deeplynx.models;
using deeplynx.interfaces;
using Npgsql;
using deeplynx.helpers;
using deeplynx.helpers.Context;

public class EventBusiness : IEventBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ICacheBusiness _cacheBusiness;
    private readonly INotificationBusiness _notificationBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    public EventBusiness(
        DeeplynxContext context, 
        ICacheBusiness cacheBusiness,
        INotificationBusiness notificationBusiness
        )
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _notificationBusiness = notificationBusiness;
    }

    /// <summary>
    /// Retrieves all project events that the user is subscribed to.
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscription belongs</param>
    /// <param name="projectId">The ID of the project to which the subscription belongs</param>
    /// <param name="organizationId">The ID of the project to which the subscription belongs</param>
    public async Task<List<EventResponseDto>> GetAllEvents(long? projectId, long? organizationId)
    {
        var eventQuery = _context.Events
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .AsQueryable();
    
        if (projectId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);   
        }

        if (organizationId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.OrganizationId == organizationId.Value);
        }
    
        return await eventQuery.Select(e => new EventResponseDto
        {
            Id = e.Id,
            Operation = e.Operation,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            EntityName = e.EntityName,
            ProjectId = e.ProjectId,
            OrganizationId = e.OrganizationId,
            DataSourceId = e.DataSourceId,
            Properties = e.Properties,
            LastUpdatedAt = e.LastUpdatedAt,
            LastUpdatedBy = e.LastUpdatedBy,
            ProjectName = e.Project != null ? e.Project.Name : null,
            DataSourceName = e.DataSource != null ? e.DataSource.Name : null
        }).ToListAsync();
    }
    
    /// <summary>
    /// Retrieves all project events for projects that the user is a member of.
    /// </summary>
    public async Task<List<EventResponseDto>> GetAllEventsByUser()
    {
        var userId = UserContextStorage.UserId;

        if (userId == 0)
        {
            return new List<EventResponseDto>();
        }

        var userProjectIds = await _context.Projects
            .Where(p => p.ProjectMembers.Any(pm =>
                pm.UserId == userId ||
                (pm.GroupId.HasValue && pm.Group != null && pm.Group.Users.Any(u => u.Id == userId))
            ))
            .Select(p => p.Id)
            .ToListAsync();
    
        if (!userProjectIds.Any())
        {
            return new List<EventResponseDto>();
        }

        var events = await _context.Events
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .Where(e => e.ProjectId.HasValue && userProjectIds.Contains(e.ProjectId.Value))
            .OrderByDescending(e => e.LastUpdatedAt)
            .Select(e => new EventResponseDto
            {
                Id = e.Id,
                Operation = e.Operation,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                EntityName = e.EntityName,
                ProjectId = e.ProjectId,
                OrganizationId = e.OrganizationId,
                DataSourceId = e.DataSourceId,
                Properties = e.Properties,
                LastUpdatedAt = e.LastUpdatedAt,
                LastUpdatedBy = e.LastUpdatedBy,
                ProjectName = e.Project != null ? e.Project.Name : null,
                DataSourceName = e.DataSource != null ? e.DataSource.Name : null
            })
            .ToListAsync();

        return events;
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

        var events = await _context.Events
            .FromSqlRaw(sql, userIdParam, projectIdParam)
            .Include(e => e.Project)       
            .Include(e => e.DataSource)
            .Select(e => new EventResponseDto
            {
                Id = e.Id,
                Operation = e.Operation,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                EntityName = e.EntityName,
                ProjectId = e.ProjectId,
                OrganizationId = e.OrganizationId,
                DataSourceId = e.DataSourceId,
                Properties = e.Properties,
                LastUpdatedAt = e.LastUpdatedAt,
                LastUpdatedBy = e.LastUpdatedBy,
                ProjectName = e.Project != null ? e.Project.Name : null,
                DataSourceName = e.DataSource != null ? e.DataSource.Name : null
            })
            .ToListAsync();
            
        return events;
    }

    /// <summary>
    /// Creates a new Event based on the event data provided.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new event to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto)
    {
        ValidationHelper.ValidateModel(dto);
        ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(dto.Operation, "Operation");
        
        var project = dto.ProjectId.HasValue ? 
            await _context.Projects.FindAsync(dto.ProjectId.Value) 
            : null;
        
        var dataSource = dto.DataSourceId.HasValue ?
            await _context.DataSources.FindAsync(dto.DataSourceId.Value)
            : null;
        
        var newEvent = new Event
        {
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            ProjectId = dto.ProjectId,
            Properties = dto.Properties,
            LastUpdatedBy = UserContextStorage.UserId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            DataSourceId = dto.DataSourceId,
            EntityId = dto.EntityId,
            EntityName = dto.EntityName
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        var response = new EventResponseDto
        {
            Id = newEvent.Id,
            Operation = newEvent.Operation,
            EntityType = newEvent.EntityType,
            EntityId = newEvent.EntityId,
            EntityName = newEvent.EntityName,
            ProjectId = newEvent.ProjectId,
            OrganizationId = newEvent.OrganizationId,
            DataSourceId = newEvent.DataSourceId,
            Properties = newEvent.Properties,
            LastUpdatedAt = newEvent.LastUpdatedAt,
            LastUpdatedBy = newEvent.LastUpdatedBy,
            ProjectName = project?.Name,
            DataSourceName = dataSource?.Name,
        };
    
        if (Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") == "true")
        {
            await _notificationBusiness.SendEventNotification(response);
        }

        return response;
    }

    /// <summary>
    /// Bulk creates Events based on the event data provided.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the event belongs</param>
    /// <param name="events">A List of data transfer objects with details on the new event to be created.</param>
    /// <returns>The list of new Events which were created.</returns>
    public async Task<List<EventResponseDto>> BulkCreateEvents(
        long projectId, 
        List<CreateEventRequestDto> events
    )
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, false);

        foreach (var dto in events)
        {
            ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
            ValidationHelper.ValidateTypes(dto.Operation, "Operation");
        }
        
        var project = await _context.Projects.FindAsync(projectId);
        var dataSource = events.First().DataSourceId != null ? 
            await _context.DataSources.FindAsync(events.First().DataSourceId) 
            : null;

        var eventEntities = events.Select(dto => new Event
        {
            ProjectId = projectId,
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            EntityName = dto.EntityName,
            Properties = dto.Properties,
            DataSourceId = dto.DataSourceId,
            LastUpdatedBy = UserContextStorage.UserId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        }).ToList();

        _context.Events.AddRange(eventEntities);
        await _context.SaveChangesAsync();

        var response = eventEntities.Select(e => new EventResponseDto
        {
            Id = e.Id,
            Operation = e.Operation,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            EntityName = e.EntityName,
            ProjectId = e.ProjectId,
            OrganizationId = e.OrganizationId,
            DataSourceId = e.DataSourceId,
            Properties = e.Properties,
            LastUpdatedAt = e.LastUpdatedAt,
            LastUpdatedBy = e.LastUpdatedBy,
            ProjectName = project?.Name,
            DataSourceName = dataSource?.Name
        }).ToList();

        if (Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") == "true")
        {
            await _notificationBusiness.SendBulkEventNotifications(response);
        }

        return response;
    }
}