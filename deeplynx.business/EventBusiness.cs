using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer.Models;
using deeplynx.models;
using deeplynx.interfaces;
using System.Text.RegularExpressions;
using Npgsql;
using deeplynx.helpers;
using Newtonsoft.Json;

public class EventBusiness : IEventBusiness
{
    private readonly DeeplynxContext _context;
    /// <summary>
    /// Initializes a new instance of the <see cref="EventBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    public EventBusiness(DeeplynxContext context)
    {
        _context = context;
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
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
            }).ToList();
    }
    
    /// <summary>
    /// Creates a new Event based on the event data provided.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new event to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, dto.ProjectId, false);
        ValidationHelper.ValidateModel(dto);
        ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
        ValidationHelper.ValidateTypes(dto.Operation, "Operation");
        
        var newEvent = new Event
        {
            Operation = dto.Operation,
            EntityType = dto.EntityType,
            ProjectId = dto.ProjectId,
            Properties = dto.Properties,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            DataSourceId = dto.DataSourceId,
            EntityId = dto.EntityId,
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return new EventResponseDto
        {
            Id = newEvent.Id,
            ProjectId = newEvent.ProjectId,
            Operation = newEvent.Operation,
            EntityType = newEvent.EntityType,
            EntityId = newEvent.EntityId,
            DataSourceId = newEvent.DataSourceId,
            Properties = newEvent.Properties,
            CreatedBy = newEvent.CreatedBy,
            CreatedAt = newEvent.CreatedAt,
        };
    }

    /// <summary>
    /// Bulk creates Events based on the event data provided.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the event belongs</param>
    /// <param name="events">A List of data transfer objects with details on the new event to be created.</param>
    /// <returns>The list of new Events which were created.</returns>
    public async Task<List<EventResponseDto>> BulkCreateEvents(long projectId, List<CreateEventRequestDto> events)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, false);

        foreach (var dto in events)
        {
            ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
            ValidationHelper.ValidateTypes(dto.Operation, "Operation");
        }

        // Bulk Insert into Events
        var sql = @"
            INSERT INTO deeplynx.events (project_id, operation, entity_type, entity_id, properties, data_source_id, created_by, created_at)
            VALUES {0}
            RETURNING *;
        ";

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };

        var utcNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        parameters.AddRange(events.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_operation", dto.Operation),
            new NpgsqlParameter($"@p{i}_entity_type", dto.EntityType),
            new NpgsqlParameter($"@p{i}_entity_id", dto.EntityId ?? (object)DBNull.Value),
            new NpgsqlParameter($"@p{i}_properties", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = dto.Properties },
            new NpgsqlParameter($"@p{i}_data_source_id", dto.DataSourceId ?? (object)DBNull.Value),
            new NpgsqlParameter($"@p{i}_created_by", dto.CreatedBy),
        }));

        var valueTuples = string.Join(", ", events.Select((dto, i) =>
            $"(@projectId, @p{i}_operation, @p{i}_entity_type, @p{i}_entity_id, @p{i}_properties, @p{i}_data_source_id, @p{i}_created_by, @now)"));

        sql = string.Format(sql, valueTuples);

        // Execute the SQL command and map results to EventResponseDto
        var result = await _context.Database
            .SqlQueryRaw<EventResponseDto>(sql, parameters.ToArray())
            .ToListAsync();

        return result;
    }
}