using deeplynx.models;
using deeplynx.datalayer.Models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long? projectId, long? organizationId);
    Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId);
    Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto);
    Task<List<EventResponseDto>> BulkCreateEvents(long projectId, List<CreateEventRequestDto> events);

    Task<List<EventResponseDto>> GetAllEventsByUser();
}
