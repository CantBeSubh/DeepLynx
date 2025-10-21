using deeplynx.datalayer.Models;

namespace deeplynx.helpers.Mappers;

public class EventMapper
{
    public EventResponseDto MapToDto(
        Event newEvent
    )
    {
        if (newEvent == null) return null;

        return new EventResponseDto
        {
            Id = newEvent.Id,
            Operation = newEvent.Operation,
            EntityType = newEvent.EntityType,
            EntityId = newEvent.EntityId,
            ProjectId = newEvent.ProjectId,
            OrganizationId = newEvent.OrganizationId,
            DataSourceId = newEvent.DataSourceId,
            Properties = newEvent.Properties,
            ProjectName = projectName,
            EntityName = entityName,
            DataSourceName = dataSourceName,
            LastUpdatedAt = newEvent.LastUpdatedAt,
            LastUpdatedBy = newEvent.LastUpdatedBy
        };
    }

    public List<EventResponseDto> MapToDtoList(
        List<Event> eventEntities,
        string? projectName = null,
        string? entityName = null,
        string? dataSourceName = null
    )
    {
        if (eventEntities == null) return new List<EventResponseDto>();

        return eventEntities.Select(e => MapToDto(
            e,
            projectName: projectName,
            entityName: entityName,
            dataSourceName: dataSourceName
        )).ToList();
    }
}
