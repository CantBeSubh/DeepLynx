using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

public class EventBusiness : IEventBusiness
{
    private readonly ICacheBusiness _cacheBusiness;
    private readonly DeeplynxContext _context;
    private readonly INotificationBusiness _notificationBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBusiness" /> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="notificationBusiness">Used for initiating notifications for subscribed users</param>
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
    /// Retrieves all events without pagination.
    /// </summary>
    /// <param name="projectId">Optional filter to only include events matching the projectId</param>
    /// <param name="organizationId">Optional filter </param>
    /// <returns>List of all events matching the filter criteria</returns>
    public async Task<List<EventResponseDto>> GetAllEvents(long? organizationId, long? projectId)
    {
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .OrderByDescending(e => e.LastUpdatedAt)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.OrganizationId == organizationId.Value);
        }
        else if (projectId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);
        }

        eventQuery = eventQuery.OrderByDescending(e => e.LastUpdatedAt);

        var items = await (from e in eventQuery
                join u in _context.Users on e.LastUpdatedBy equals u.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new EventResponseDto
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
                    LastUpdatedByUserName = user != null ? user.Name : null,
                    ProjectName = e.ProjectId != null ? e.Project.Name : null,
                    DataSourceName = e.DataSourceId != null ? e.DataSource.Name : null,
                    OrganizationName = e.OrganizationId != null ? e.Organization.Name : null
                })
            .ToListAsync();

        return items;
    }

    /// <summary>
    /// Retrieves all project events with pagination.
    /// </summary>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <param name="organizationId">The id of the organization the events are a part of</param>
    /// <param name="projectId">The id of the project the events are a part of</param>
    /// <returns>Paginated response containing events and pagination metadata</returns>
    public async Task<PaginatedResponse<EventResponseDto>> QueryAllEvents(EventsQueryRequestDTO? queryDto,
        long? organizationId, long? projectId)
    {
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.OrganizationId == organizationId.Value);
        }
        else if (projectId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);
        }

        eventQuery = eventQuery.OrderByDescending(e => e.LastUpdatedAt);

        if (queryDto != null)
        {
            if (queryDto.lastUpdatedBy.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.LastUpdatedBy == queryDto.lastUpdatedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.operation))
            {
                var searchTerm = queryDto.operation.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.Operation, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.entityType))
            {
                var searchTerm = queryDto.entityType.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityType, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.entityName))
            {
                var searchTerm = queryDto.entityName.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityName, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.dataSourceName))
            {
                var searchTerm = queryDto.dataSourceName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.DataSource != null &&
                    EF.Functions.ILike(e.DataSource.Name, $"%{searchTerm}%"));
            }

            if (queryDto.startDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt >= queryDto.startDate.Value);

            if (queryDto.endDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt <= queryDto.endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await eventQuery.CountAsync();

        // Get pagination values
        var pageNumber = queryDto?.PageNumber ?? 1;
        var pageSize = queryDto?.GetValidatedPageSize() ?? 500;

        // Apply pagination and execute query
        var items = await (from e in eventQuery
                join u in _context.Users on e.LastUpdatedBy equals u.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new EventResponseDto
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
                    LastUpdatedByUserName = user != null ? user.Name : null,
                    ProjectName = e.ProjectId != null ? e.Project.Name : null,
                    DataSourceName = e.DataSourceId != null ? e.DataSource.Name : null,
                    OrganizationName = e.OrganizationId != null ? e.Organization.Name : null
                })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<EventResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Retrieves all project events for projects that the user is a member of, with pagination.
    /// </summary>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <returns>Paginated response containing events and pagination metadata</returns>
    public async Task<PaginatedResponse<EventResponseDto>> QueryAuthorizedEvents(long currentUserId,
        EventsQueryRequestDTO? queryDto, long? organizationId, long? projectId)
    {
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            // Get all project IDs the user has access to within this organization
            var userProjectIds = await _context.Projects
                .Where(p => p.OrganizationId == organizationId &&
                            p.ProjectMembers.Any(pm =>
                                pm.UserId == currentUserId ||
                                (pm.GroupId.HasValue && pm.Group != null &&
                                 pm.Group.Users.Any(u => u.Id == currentUserId))
                            ))
                .Select(p => p.Id)
                .ToListAsync();

            eventQuery = eventQuery.Where(e =>
                (e.OrganizationId == organizationId.Value) ||
                (e.ProjectId.HasValue && userProjectIds.Contains(e.ProjectId.Value))
            );

            if (projectId.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);
            }
        }
        else if (projectId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);
        }

        eventQuery = eventQuery.OrderByDescending(e => e.LastUpdatedAt);

        if (queryDto != null)
        {
            if (!string.IsNullOrWhiteSpace(queryDto.projectName))
            {
                var searchTerm = queryDto.projectName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.Project != null &&
                    EF.Functions.ILike(e.Project.Name, $"%{searchTerm}%"));
            }

            if (queryDto.lastUpdatedBy.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.LastUpdatedBy == queryDto.lastUpdatedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.operation))
            {
                var searchTerm = queryDto.operation.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.Operation, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.entityType))
            {
                var searchTerm = queryDto.entityType.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityType, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.entityName))
            {
                var searchTerm = queryDto.entityName.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityName, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.dataSourceName))
            {
                var searchTerm = queryDto.dataSourceName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.DataSource != null &&
                    EF.Functions.ILike(e.DataSource.Name, $"%{searchTerm}%"));
            }

            if (queryDto.startDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt >= queryDto.startDate.Value);

            if (queryDto.endDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt <= queryDto.endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await eventQuery.CountAsync();

        // Get pagination values
        var pageNumber = queryDto?.PageNumber ?? 1;
        var pageSize = queryDto?.GetValidatedPageSize() ?? 25;

        // Apply pagination and execute query
        var items = await (from e in eventQuery
                join u in _context.Users on e.LastUpdatedBy equals u.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new EventResponseDto
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
                    LastUpdatedByUserName = user != null ? user.Name : null,
                    ProjectName = e.Project != null ? e.Project.Name : null,
                    DataSourceName = e.DataSource != null ? e.DataSource.Name : null
                })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<EventResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Queries all events that the user is subscribed to at both organization and project levels.
    /// Organization-level subscriptions (projectId is null) match all events in the organization.
    /// Project-level subscriptions (projectId is not null) match only events in that specific project.
    /// </summary>
    /// <param name="currentUserId">The ID of the user to which the subscription belongs</param>
    /// <param name="organizationId">The ID of the organization</param>
    /// <param name="projectId">Optional ID of the project to filter events</param>
    public async Task<PaginatedResponse<EventResponseDto>> QueryEventsBySubscriptions(long currentUserId,
        EventsQueryRequestDTO? queryDto, long? organizationId, long? projectId)
    {
        var subscriptionsQuery = _context.Set<Subscription>()
            .Where(s => s.UserId == currentUserId && s.OrganizationId == organizationId);

        if (projectId.HasValue)
        {
            subscriptionsQuery = subscriptionsQuery.Where(s =>
                s.ProjectId == projectId.Value);
        }
        else
        {
            subscriptionsQuery = subscriptionsQuery.Where(s => s.ProjectId == null);
        }

        var subscriptions = await subscriptionsQuery.ToListAsync();

        if (!subscriptions.Any())
            return new PaginatedResponse<EventResponseDto>
            {
                Items = new List<EventResponseDto>(),
                PageNumber = queryDto?.PageNumber ?? 1,
                PageSize = queryDto?.GetValidatedPageSize() ?? 25,
                TotalCount = 0
            };

        string sql;

        if (projectId.HasValue)
        {
            sql = @"
                SELECT DISTINCT e.*
                FROM deeplynx.events e
                INNER JOIN deeplynx.subscriptions s
                    ON s.user_id = @currentUserId
                    AND s.project_id = @projectId
                WHERE e.project_id = @projectId
                    AND ((s.entity_id = e.entity_id) OR s.entity_id IS NULL)
                    AND ((s.entity_type = e.entity_type) OR s.entity_type IS NULL)
                    AND ((s.data_source_id = e.data_source_id) OR s.data_source_id IS NULL)
                    AND ((s.operation = e.operation) OR s.operation IS NULL)";
        }
        else
        {
            sql = @"
                SELECT DISTINCT e.*
                FROM deeplynx.events e
                INNER JOIN deeplynx.subscriptions s
                    ON s.user_id = @currentUserId
                    AND s.organization_id = @organizationId
                    AND s.project_id IS NULL
                WHERE e.organization_id = @organizationId
                    AND ((s.entity_id = e.entity_id) OR s.entity_id IS NULL)
                    AND ((s.entity_type = e.entity_type) OR s.entity_type IS NULL)
                    AND ((s.data_source_id = e.data_source_id) OR s.data_source_id IS NULL)
                    AND ((s.operation = e.operation) OR s.operation IS NULL)";
        }

        var currentUserIdParam = new NpgsqlParameter("currentUserId", currentUserId);
        var organizationIdParam = new NpgsqlParameter("organizationId", organizationId);
        var projectIdParam = new NpgsqlParameter("projectId", (object?)projectId ?? DBNull.Value);

        var eventQuery = _context.Events
            .FromSqlRaw(sql, currentUserIdParam, organizationIdParam, projectIdParam)
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .AsQueryable();

        if (queryDto != null)
        {
            if (queryDto.lastUpdatedBy.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.LastUpdatedBy == queryDto.lastUpdatedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.projectName))
            {
                var searchTerm = queryDto.projectName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.Project != null &&
                    EF.Functions.ILike(e.Project.Name, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.operation))
            {
                var searchTerm = queryDto.operation.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.Operation, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.entityType))
            {
                var searchTerm = queryDto.entityType.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityType, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.entityName))
            {
                var searchTerm = queryDto.entityName.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityName, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.dataSourceName))
            {
                var searchTerm = queryDto.dataSourceName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.DataSource != null &&
                    EF.Functions.ILike(e.DataSource.Name, $"%{searchTerm}%"));
            }

            if (queryDto.startDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt >= queryDto.startDate.Value);

            if (queryDto.endDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt <= queryDto.endDate.Value);
        }

        eventQuery = eventQuery.OrderByDescending(e => e.LastUpdatedAt);

        var totalCount = await eventQuery.CountAsync();

        var pageNumber = queryDto?.PageNumber ?? 1;
        var pageSize = queryDto?.GetValidatedPageSize() ?? 25;

        var items = await (from e in eventQuery
                join u in _context.Users on e.LastUpdatedBy equals u.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new EventResponseDto
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
                    LastUpdatedByUserName = user != null ? user.Name : null,
                    ProjectName = e.ProjectId != null ? e.Project.Name : null,
                    DataSourceName = e.DataSourceId != null ? e.DataSource.Name : null,
                    OrganizationName = e.OrganizationId != null ? e.Organization.Name : null
                })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<EventResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates a new Event based on the event data provided.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="dto">A data transfer object with details on the new event to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(long currentUserId, CreateEventRequestDto dto, long? organizationId, long? projectId)
    {
        ValidationHelper.ValidateModel(dto);
        ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(dto.Operation, "Operation");

        if ((organizationId.HasValue && projectId.HasValue) || (!organizationId.HasValue && !projectId.HasValue))
        {
            throw new ArgumentException("Exactly one of OrganizationId or ProjectId must be provided.");
        }

        var project = projectId != null
            ? await _context.Projects.FindAsync(projectId)
            : null;

        var organization = await _context.Organizations.FindAsync(organizationId);

        var dataSource = dto.DataSourceId.HasValue
            ? await _context.DataSources.FindAsync(dto.DataSourceId.Value)
            : null;

        var newEvent = new Event
        {
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            OrganizationId = organizationId,
            ProjectId = projectId,
            Properties = dto.Properties,
            LastUpdatedBy = currentUserId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            DataSourceId = dto.DataSourceId,
            EntityId = dto.EntityId,
            EntityName = dto.EntityName,
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
            OrganizationName = organization?.Name
        };

        if (Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") == "true")
            await _notificationBusiness.SendEventNotification(response);

        return response;
    }

    /// <summary>
    /// Creates a single Event for a bulk operation with the total count in properties.
    /// </summary>
    /// <param name="currentUserId">The id of the organization this event occurred in</param>
    /// <param name="organizationId">The id of the organization this event occurred in</param>
    /// <param name="events">A List of data transfer objects representing the bulk operation</param>
    /// <param name="projectId">The ID of the project to which the event belongs</param>
    /// <returns>The single Event created for the bulk operation.</returns>
    public async Task<EventResponseDto> BulkCreateEvents(
        long currentUserId,
        List<CreateEventRequestDto> events,
        long? organizationId,
        long? projectId = null
    )
    {
        if (!events.Any())
            throw new ArgumentException("Events list cannot be empty");

        var firstEvent = events.First();

        ValidationHelper.ValidateTypes(firstEvent.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(firstEvent.Operation, "Operation");

        // Validate XOR constraint
        if ((organizationId.HasValue && projectId.HasValue) || (!organizationId.HasValue && !projectId.HasValue))
        {
            throw new ArgumentException("Exactly one of OrganizationId or ProjectId must be provided.");
        }

        var project = projectId != null
            ? await _context.Projects.FindAsync(projectId)
            : null;

        var organization = await _context.Organizations.FindAsync(organizationId);

        var dataSource = firstEvent.DataSourceId.HasValue
            ? await _context.DataSources.FindAsync(firstEvent.DataSourceId.Value)
            : null;

        Dictionary<string, object> properties;

        if (!string.IsNullOrWhiteSpace(firstEvent.Properties))
        {
            properties = JsonSerializer.Deserialize<Dictionary<string, object>>(firstEvent.Properties)
                         ?? new Dictionary<string, object>();
        }
        else
        {
            properties = new Dictionary<string, object>();
        }

        properties["BulkCount"] = events.Count;

        var propertiesJson = JsonSerializer.Serialize(properties);

        var newEvent = new Event
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            Operation = firstEvent.Operation,
            EntityType = firstEvent.EntityType,
            EntityId = firstEvent.EntityId,
            EntityName = firstEvent.EntityName,
            Properties = propertiesJson,
            DataSourceId = firstEvent.DataSourceId,
            LastUpdatedBy = currentUserId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
            OrganizationName = organization?.Name
        };

        if (Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") == "true")
            await _notificationBusiness.SendEventNotification(response);

        return response;
    }
}