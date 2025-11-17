using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    public async Task<List<EventResponseDto>> GetAllEvents(long organizationId, long? projectId)
    {
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.LastUpdatedAt)
            .AsQueryable();

        if (projectId.HasValue)
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);

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
                    DataSourceName = e.DataSource != null ? e.DataSource.Name : null,
                    OrganizationName = e.Organization != null ? e.Organization.Name : null
                })
            .ToListAsync();

        return items;
    }

    /// <summary>
    /// Retrieves all project events with pagination.
    /// </summary>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <returns>Paginated response containing events and pagination metadata</returns>
    public async Task<PaginatedResponse<EventResponseDto>> QueryEvents(long organizationId,
        EventsQueryRequestDTO? queryDto, long? projectId)
    {
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.LastUpdatedAt)
            .AsQueryable();

        if (projectId != null)
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId);

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
                    ProjectName = e.Project != null ? e.Project.Name : null,
                    DataSourceName = e.DataSource != null ? e.DataSource.Name : null,
                    OrganizationName = e.Organization != null ? e.Organization.Name : null
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
    public async Task<PaginatedResponse<EventResponseDto>> QueryEventsByUser(long organizationId, long userId,
        EventsQueryRequestDTO? queryDto, long? projectId)
    {
        // First, verify user is a member of the organization
        var isOrgMember = await _context.OrganizationUsers
            .AnyAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId);

        if (!isOrgMember)
            return new PaginatedResponse<EventResponseDto>
            {
                Items = new List<EventResponseDto>(),
                PageNumber = queryDto?.PageNumber ?? 1,
                PageSize = queryDto?.GetValidatedPageSize() ?? 25,
                TotalCount = 0
            };

        // Get all project IDs the user has access to within this organization
        var userProjectIds = await _context.Projects
            .Where(p => p.OrganizationId == organizationId &&
                        p.ProjectMembers.Any(pm =>
                            pm.UserId == userId ||
                            (pm.GroupId.HasValue && pm.Group != null && pm.Group.Users.Any(u => u.Id == userId))
                        ))
            .Select(p => p.Id)
            .ToListAsync();

        // If a specific projectId is provided, verify user has access to it
        if (projectId.HasValue)
        {
            if (!userProjectIds.Contains(projectId.Value))
                return new PaginatedResponse<EventResponseDto>
                {
                    Items = new List<EventResponseDto>(),
                    PageNumber = queryDto?.PageNumber ?? 1,
                    PageSize = queryDto?.GetValidatedPageSize() ?? 25,
                    TotalCount = 0
                };
        }

        // Build the event query
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.LastUpdatedAt)
            .AsQueryable();

        // Filter by project access
        // Events can have no project (ProjectId is null) OR have a project the user has access to
        eventQuery = eventQuery.Where(e =>
            !e.ProjectId.HasValue ||
            userProjectIds.Contains(e.ProjectId.Value));

        // If specific projectId provided, filter to only that project
        if (projectId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);
        }

        // Apply additional filters from queryDto
        if (queryDto != null)
        {
            if (projectId != null)
                eventQuery = eventQuery.Where(e => e.ProjectId == projectId);

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
                    DataSourceName = e.DataSource != null ? e.DataSource.Name : null,
                    OrganizationName = e.Organization != null ? e.Organization.Name : null
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
    /// Retrieves all events that the user is subscribed to at both organization and project levels.
    /// Organization-level subscriptions (projectId is null) match all events in the organization.
    /// Project-level subscriptions (projectId is not null) match only events in that specific project.
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscription belongs</param>
    /// <param name="organizationId">The ID of the organization</param>
    /// <param name="projectId">Optional ID of the project to filter events</param>
    public async Task<List<EventResponseDto>> GetAllEventsByUserSubscriptions(long organizationId, long userId, 
    long? projectId)
    {
        // First, verify user is a member of the organization
        var isOrgMember = await _context.OrganizationUsers
            .AnyAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId);

        if (!isOrgMember)
            return new List<EventResponseDto>();

        // If projectId is provided, verify user has access to that specific project
        if (projectId.HasValue)
        {
            var isProjectMember = await _context.Projects
                .Where(p => p.Id == projectId.Value && p.OrganizationId == organizationId)
                .AnyAsync(p => p.ProjectMembers.Any(pm =>
                    pm.UserId == userId ||
                    (pm.GroupId.HasValue && pm.Group != null && pm.Group.Users.Any(u => u.Id == userId))
                ));

            if (!isProjectMember)
                return new List<EventResponseDto>();
        }

        // Get subscriptions based on organizationId and projectId
        var subscriptionsQuery = _context.Set<Subscription>()
            .Where(s => s.UserId == userId && s.OrganizationId == organizationId);

        if (projectId.HasValue)
        {
            // Get subscriptions that match the specific project
            subscriptionsQuery = subscriptionsQuery.Where(s => 
                s.ProjectId == projectId.Value);
        }
        else
        {
            // Only get organization-level subscriptions (no project)
            subscriptionsQuery = subscriptionsQuery.Where(s => s.ProjectId == null);
        }

        var subscriptions = await subscriptionsQuery.ToListAsync();

        if (!subscriptions.Any()) 
            return new List<EventResponseDto>();

        // Build SQL based on whether we're filtering by project or not
        string sql;
        
        if (projectId.HasValue)
        {
            // Get events that match project-level subscriptions
            sql = @"
                SELECT DISTINCT e.*
                FROM deeplynx.events e
                INNER JOIN deeplynx.subscriptions s
                    ON s.organization_id = e.organization_id
                    AND s.user_id = @userId
                    AND s.organization_id = @organizationId
                    AND s.project_id = @projectId
                WHERE e.project_id = @projectId
                    AND ((s.entity_id = e.entity_id) OR s.entity_id IS NULL)
                    AND ((s.entity_type = e.entity_type) OR s.entity_type IS NULL)
                    AND ((s.data_source_id = e.data_source_id) OR s.data_source_id IS NULL)
                    AND ((s.operation = e.operation) OR s.operation IS NULL)";
        }
        else
        {
            // Get events that match organization-level subscriptions (events with no project)
            sql = @"
                SELECT DISTINCT e.*
                FROM deeplynx.events e
                INNER JOIN deeplynx.subscriptions s
                    ON s.organization_id = e.organization_id
                    AND s.user_id = @userId
                    AND s.organization_id = @organizationId
                    AND s.project_id IS NULL
                WHERE e.project_id IS NULL
                    AND ((s.entity_id = e.entity_id) OR s.entity_id IS NULL)
                    AND ((s.entity_type = e.entity_type) OR s.entity_type IS NULL)
                    AND ((s.data_source_id = e.data_source_id) OR s.data_source_id IS NULL)
                    AND ((s.operation = e.operation) OR s.operation IS NULL)";
        }

        var userIdParam = new NpgsqlParameter("userId", userId);
        var organizationIdParam = new NpgsqlParameter("organizationId", organizationId);
        var projectIdParam = new NpgsqlParameter("projectId", (object?)projectId ?? DBNull.Value);

        var events = await _context.Events
            .FromSqlRaw(sql, userIdParam, organizationIdParam, projectIdParam)
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
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
                DataSourceName = e.DataSource != null ? e.DataSource.Name : null,
                OrganizationName = e.Organization != null ? e.Organization.Name : null
            })
            .ToListAsync();

        return events;
    }

    /// <summary>
    /// Creates a new Event based on the event data provided.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new event to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(long OrganizationId, long userId, CreateEventRequestDto dto, long? projectId)
    {
        ValidationHelper.ValidateModel(dto);
        ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(dto.Operation, "Operation");

        var project = projectId
            ? await _context.Projects.FindAsync(projectId)
            : null;

        var dataSource = dto.DataSourceId.HasValue
            ? await _context.DataSources.FindAsync(dto.DataSourceId.Value)
            : null;

        var newEvent = new Event
        {
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            OrganizationId = dto.OrganizationId,
            ProjectId = projectId,
            Properties = dto.Properties,
            LastUpdatedBy = userId,
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
            OrganizationName = e.Organization != null ? e.Organization.Name : null
        };

        if (Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") == "true")
            await _notificationBusiness.SendEventNotification(response);

        return response;
    }

    /// <summary>
    /// Bulk creates Events based on the event data provided.
    /// </summary>
    /// <param name="organizationId">The id of the organization these events occured in</param>
    /// <param name="events">A List of data transfer objects with details on the new event to be created.</param>
    /// <param name="projectId">The ID of the project to which the event belongs</param>
    /// <returns>The list of new Events which were created.</returns>
    public async Task<List<EventResponseDto>> BulkCreateEvents(
        long organizationId,
        List<CreateEventRequestDto> events,
        long? projectId
    )
    {
        foreach (var dto in events)
        {
            ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
            ValidationHelper.ValidateTypes(dto.Operation, "Operation");
        }

        var project = projectId.HasValue
            ? await _context.Projects.FindAsync(projectId.Value)
            : null;

        var dataSource = events.First().DataSourceId != null
            ? await _context.DataSources.FindAsync(events.First().DataSourceId)
            : null;

        var eventEntities = events.Select(dto => new Event
        {
            OrganizationId = organizationId, // Same for all events in the batch
            ProjectId = projectId, // Same for all events in the batch (can be null)
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            EntityName = dto.EntityName,
            Properties = dto.Properties,
            DataSourceId = dto.DataSourceId,
            LastUpdatedBy = UserContextStorage.UserId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            OrganizationName = e.Organization != null ? e.Organization.Name : null,
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
            await _notificationBusiness.SendBulkEventNotifications(response);

        return response;
    }
}