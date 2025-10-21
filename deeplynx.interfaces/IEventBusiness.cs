using deeplynx.models;
using deeplynx.datalayer.Models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long? projectId, long? organizationId);
    Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId);
    Task<List<EventResponseDto>> GetAllEventsByUser();
    
    Task<EventResponseDto> CreateEvent(
        CreateEventRequestDto dto,
        string? projectName = null,
        string? entityName = null,
        string? dataSourceName = null
    );
    
    Task<List<EventResponseDto>> BulkCreateEvents(
        long projectId, 
        List<CreateEventRequestDto> events,
        string? projectName = null,
        string? entityName = null,
        string? dataSourceName = null
    );
}