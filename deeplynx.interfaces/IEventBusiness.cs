using deeplynx.models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(EventsQueryRequestDTO? filterDto);
    Task<PaginatedResponse<EventResponseDto>> GetAllEventsPaginated(EventsQueryRequestDTO? filterDto);
    Task<PaginatedResponse<EventResponseDto>> GetAllEventsByUserPaginated(EventsQueryRequestDTO? filterDto);
    Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId);
    Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto);
    Task<List<EventResponseDto>> BulkCreateEvents(long projectId, List<CreateEventRequestDto> events);
}