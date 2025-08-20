using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer.Models;
using deeplynx.models;
using deeplynx.interfaces;
using System.Text.RegularExpressions;
using Npgsql;
using deeplynx.helpers;


public class EventBusiness : IEventBusiness
{
    private readonly DeeplynxContext _context;
    private static readonly List<string> AllowedEntityTypes = new List<string>
    {
        "class", 
        "data_source", 
        "relationship", 
        "project", 
        "edge", 
        "edge_mapping", 
        "record", 
        "record_mapping",
        "metadata", 
        "user", 
        "tag"
    };

    private static readonly List<string> AllowedOperations = new List<string>
    {
        "create",
        "update",
        "delete",
    };
    
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
    /// <param name="dto">A data transfer object with details on the new class to be created.</param>
    /// <returns>The new Event which was just created.</returns>
    public async Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, dto.ProjectId);
        ValidationHelper.ValidateModel(dto);
        Validate(dto.EntityType, "EntityType");
        Validate(dto.Operation, "Operation");
        
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
    
    private string Validate(string value, string type)
    {
        if (type == "EntityType")
        {
            if (string.IsNullOrEmpty(value) || !AllowedEntityTypes.Contains(value))
            {
                throw new ArgumentException($"EntityType must be one of {string.Join(", ", AllowedEntityTypes)}");
            }

            return value;
        }

        if (type == "Operation")
        {
            if (string.IsNullOrEmpty(value) || !AllowedOperations.Contains(value))
            {
                throw new ArgumentException($"Operation must be one of {string.Join(", ", AllowedOperations)}");
            }

            return value;
        }
            
        throw new ArgumentException("Invalid type for validation.");
    }
}