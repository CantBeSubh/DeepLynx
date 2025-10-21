using deeplynx.datalayer.Models;

namespace deeplynx.helpers.Mappers;

public class EventMapper
{
    public EventResponseDto MapToDto(
        Event eventEntity,
        string? projectName = null,
        string? entityName = null,
        string? dataSourceName = null
    )
    {
        if (eventEntity == null) return null;

        return new EventResponseDto
        {
            Id = eventEntity.Id,
            Operation = eventEntity.Operation,
            EntityType = eventEntity.EntityType,
            EntityId = eventEntity.EntityId,
            ProjectId = eventEntity.ProjectId,
            OrganizationId = eventEntity.OrganizationId,
            DataSourceId = eventEntity.DataSourceId,
            Properties = eventEntity.Properties,
            ProjectName = projectName,
            EntityName = entityName,
            DataSourceName = dataSourceName,
            LastUpdatedAt = eventEntity.LastUpdatedAt,
            LastUpdatedBy = eventEntity.LastUpdatedBy
        };
    }
}
