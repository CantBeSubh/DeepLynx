using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using NpgsqlTypes;

public class EventBusiness : IEventBusiness
{
    private readonly IBulkCopyUpsertExecutor _bulkCopyUpsertExecutor;
    private readonly ICacheBusiness _cacheBusiness;
    private readonly DeeplynxContext _context;
    private readonly INotificationBusiness _notificationBusiness;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventBusiness" /> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// <param name="notificationBusiness">Used for initiating notifications for subscribed users</param>
    /// <param name="bulkCopyUpsertExecutor">Used to access bulk upsert operations</param>
    public EventBusiness(
        DeeplynxContext context,
        INotificationBusiness notificationBusiness,
        IBulkCopyUpsertExecutor bulkCopyUpsertExecutor
    )
    {
        _context = context;
        _notificationBusiness = notificationBusiness;
        _bulkCopyUpsertExecutor = bulkCopyUpsertExecutor;
    }

    /// <summary>
    ///     Retrieves all events without pagination.
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
    ///     Retrieves all project events with pagination.
    /// </summary>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <param name="organizationId">The id of the organization the events are a part of</param>
    /// <param name="projectId">The id of the project the events are a part of</param>
    /// <returns>Paginated response containing events and pagination metadata</returns>
    public async Task<PaginatedResponse<EventResponseDto>> QueryAllEvents(
        long organizationId, long? projectId, EventsQueryRequestDto? queryDto)
    {
        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .AsQueryable();

        eventQuery = eventQuery.Where(e => e.OrganizationId == organizationId);

        if (projectId.HasValue)
        {
            eventQuery = eventQuery.Where(e => e.ProjectId == projectId.Value);
        }

        eventQuery = eventQuery.OrderByDescending(e => e.LastUpdatedAt);

        if (queryDto != null)
        {
            if (queryDto.LastUpdatedBy.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.LastUpdatedBy == queryDto.LastUpdatedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Operation))
            {
                var searchTerm = queryDto.Operation.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.Operation, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.EntityType))
            {
                var searchTerm = queryDto.EntityType.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityType, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.EntityName))
            {
                var searchTerm = queryDto.EntityName.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityName, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.DataSourceName))
            {
                var searchTerm = queryDto.DataSourceName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.DataSource != null &&
                    EF.Functions.ILike(e.DataSource.Name, $"%{searchTerm}%"));
            }

            if (queryDto.StartDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt >= queryDto.StartDate.Value);

            if (queryDto.EndDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt <= queryDto.EndDate.Value);
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
    ///     Retrieves all project events for projects that the user is a member of, with pagination.
    /// </summary>
    /// <param name="queryDto">Filter criteria and pagination parameters</param>
    /// <returns>Paginated response containing events and pagination metadata</returns>
    public async Task<PaginatedResponse<EventResponseDto>> QueryAuthorizedEvents(long currentUserId,
        long organizationId, long[] projectIds, EventsQueryRequestDto? queryDto)
    {
        var requestingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        bool isSysAdmin = requestingUser?.IsSysAdmin ?? false;

        var eventQuery = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Project)
            .Include(e => e.DataSource)
            .AsQueryable();

        List<long> userProjectIds;
        // Get all project IDs the user has access to within this organization
        if (!isSysAdmin)
        {
            userProjectIds = await _context.Projects
                .Where(p => p.OrganizationId == organizationId &&
                            p.ProjectMembers.Any(pm =>
                                pm.UserId == currentUserId ||
                                (pm.GroupId.HasValue && pm.Group != null &&
                                 pm.Group.Users.Any(u => u.Id == currentUserId))
                            ))
                .Select(p => p.Id)
                .ToListAsync();
        }
        // if user is system admin then return all projects regardless if they are an official member
        else
        {
            userProjectIds = await _context.Projects.Select(p => p.Id).ToListAsync();
        }

        eventQuery = eventQuery.Where(e =>
            (e.OrganizationId == organizationId) ||
            (e.ProjectId.HasValue && userProjectIds.Contains(e.ProjectId.Value))
        );

        if (projectIds.Length > 0)
        {
            var projectIdsList = projectIds.AsEnumerable();
            eventQuery = eventQuery.Where(e =>
                e.ProjectId.HasValue && projectIdsList.Contains(e.ProjectId.Value));
        }

        eventQuery = eventQuery.OrderByDescending(e => e.LastUpdatedAt);

        if (queryDto != null)
        {
            if (!string.IsNullOrWhiteSpace(queryDto.ProjectName))
            {
                var searchTerm = queryDto.ProjectName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.Project != null &&
                    EF.Functions.ILike(e.Project.Name, $"%{searchTerm}%"));
            }

            if (queryDto.LastUpdatedBy.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.LastUpdatedBy == queryDto.LastUpdatedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Operation))
            {
                var searchTerm = queryDto.Operation.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.Operation, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.EntityType))
            {
                var searchTerm = queryDto.EntityType.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityType, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.EntityName))
            {
                var searchTerm = queryDto.EntityName.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityName, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.DataSourceName))
            {
                var searchTerm = queryDto.DataSourceName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.DataSource != null &&
                    EF.Functions.ILike(e.DataSource.Name, $"%{searchTerm}%"));
            }

            if (queryDto.StartDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt >= queryDto.StartDate.Value);

            if (queryDto.EndDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt <= queryDto.EndDate.Value);
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
        long organizationId, long? projectId, EventsQueryRequestDto? queryDto)
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
            if (queryDto.LastUpdatedBy.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.LastUpdatedBy == queryDto.LastUpdatedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.ProjectName))
            {
                var searchTerm = queryDto.ProjectName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.Project != null &&
                    EF.Functions.ILike(e.Project.Name, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Operation))
            {
                var searchTerm = queryDto.Operation.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.Operation, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.EntityType))
            {
                var searchTerm = queryDto.EntityType.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityType, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.EntityName))
            {
                var searchTerm = queryDto.EntityName.Trim();
                eventQuery = eventQuery.Where(e =>
                    EF.Functions.ILike(e.EntityName, $"%{searchTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.DataSourceName))
            {
                var searchTerm = queryDto.DataSourceName.Trim();
                eventQuery = eventQuery.Where(e =>
                    e.DataSource != null &&
                    EF.Functions.ILike(e.DataSource.Name, $"%{searchTerm}%"));
            }

            if (queryDto.StartDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt >= queryDto.StartDate.Value);

            if (queryDto.EndDate.HasValue)
                eventQuery = eventQuery.Where(e => e.LastUpdatedAt <= queryDto.EndDate.Value);
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
    ///     Creates a new Event based on the event data provided.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="organizationId">ID of the organization the event belongs to.</param>
    /// <param name="projectId">ID of the project the event belongs to.</param>
    /// <param name="dto">A data transfer object with details on the new event to be created.</param>
    /// <param name="count">A data transfer object with details on the new event to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(long currentUserId, long organizationId, long? projectId,
        CreateEventRequestDto dto, long? count = 1)
    {
        ValidationHelper.ValidateModel(dto);
        ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(dto.Operation, "Operation");

        var project = projectId != null
            ? await _context.Projects.FindAsync(projectId)
            : null;

        var organization = await _context.Organizations.FindAsync(organizationId);

        var dataSource = dto.DataSourceId.HasValue
            ? await _context.DataSources.FindAsync(dto.DataSourceId.Value)
            : null;

        Dictionary<string, object> properties;

        if (!string.IsNullOrWhiteSpace(dto.Properties))
        {
            properties = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.Properties)
                         ?? new Dictionary<string, object>();
        }
        else
        {
            properties = new Dictionary<string, object>();
        }

        properties["Count"] = count.Value;

        var propertiesJson = JsonSerializer.Serialize(properties);

        var newEvent = new Event
        {
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            OrganizationId = organizationId,
            ProjectId = projectId,
            Properties = propertiesJson,
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
        var dataSource = events.First().DataSourceId != null
            ? await _context.DataSources.FindAsync(events.First().DataSourceId)
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
            await _notificationBusiness.SendBulkEventNotifications(response);

        return response;
    }
     /// <summary>
    ///     Create a new record
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project under which to create the record</param>
    /// <param name="dataSourceId">The ID of the data source under which to create the record</param>
    /// <param name="dto">The data transfer object containing details on the record to be created</param>
    /// <returns>The newly created metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if the project or datasource are not found</exception>
    /// <exception cref="Exception">Returned if the metadata is too deeply nested</exception>
    public async Task<RecordResponseDto> CreateRecord(long currentUserId, long organizationId, long projectId,
        long dataSourceId,
        CreateRecordRequestDto dto)
    {
        ValidationHelper.ValidateModel(dto);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context, dataSourceId, projectId);

        if (dto.Properties == null)
            throw new ArgumentNullException(nameof(dto.Properties), "Properties cannot be null");

        var maxDepth = CalculateJsonMaxDepth(dto.Properties);
        if (maxDepth > 3)
            throw new Exception(
                $"The depth of the JSON structure exceeds the maximum allowed depth of 3. Current depth of properties is {maxDepth}.");

        if (dto.ObjectStorageId != null) await CheckObjectStorageExists(projectId, dto.ObjectStorageId.Value);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var record = new Record
            {
                ProjectId = projectId,
                DataSourceId = dataSourceId,
                Uri = dto.Uri,
                ObjectStorageId = dto.ObjectStorageId,
                Properties = dto.Properties.ToString()!,
                OriginalId = dto.OriginalId,
                Name = dto.Name,
                Description = dto.Description,
                ClassId = dto.ClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = currentUserId,
                FileType = dto.FileType,
                OrganizationId = organizationId
            };

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            // We need to handle tag creation/linking separate of record object save
            var tags = await ProcessTags(currentUserId, organizationId, projectId, record.Id, dto.Tags);

            // Log Record Create Event
            await _eventBusiness.CreateEvent(
                currentUserId,
                organizationId,
                projectId,
                new CreateEventRequestDto
                {
                    EntityType = "record",
                    EntityId = record.Id,
                    EntityName = record.Name,
                    Operation = "create",
                    Properties = "{}",
                    DataSourceId = record.DataSourceId
                });

            await transaction.CommitAsync();

            return new RecordResponseDto
            {
                Id = record.Id,
                Description = record.Description,
                Uri = record.Uri,
                Properties = record.Properties,
                ObjectStorageId = record.ObjectStorageId,
                OriginalId = record.OriginalId,
                Name = record.Name,
                ClassId = record.ClassId,
                DataSourceId = record.DataSourceId,
                ProjectId = record.ProjectId,
                OrganizationId = record.OrganizationId,
                LastUpdatedBy = record.LastUpdatedBy,
                LastUpdatedAt = record.LastUpdatedAt,
                IsArchived = record.IsArchived,
                FileType = record.FileType,
                Tags = tags
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new Exception("Unable to create record and/or create/attach tags");
        }
    }
    
}