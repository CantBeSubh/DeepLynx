using deeplynx.models;
using deeplynx.datalayer.Models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<PaginatedResponse<EventResponseDto>> GetAllEvents(EventFilterRequestDTO? filterDto);
    Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId);
    Task<List<EventResponseDto>> GetAllEventsByUser(EventFilterRequestDTO? filterDto);
    Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto);
    Task<List<EventResponseDto>> BulkCreateEvents(long projectId, List<CreateEventRequestDto> events
    );
}