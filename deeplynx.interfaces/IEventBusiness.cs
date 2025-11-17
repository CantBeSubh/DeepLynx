using deeplynx.models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long organizationId, long? projectId);
    Task<PaginatedResponse<EventResponseDto>> QueryEvents(long organizationId, EventsQueryRequestDTO? filterDto, long? projectId);
    Task<PaginatedResponse<EventResponseDto>> QueryEventsByUser(long organizationId, long userId, EventsQueryRequestDTO? filterDto, long? projectId);
    Task<List<EventResponseDto>> GetAllEventsByUserSubscriptions(long organizationId, long userId, long? projectId);
    Task<PaginatedResponse<EventResponseDto>> QueryEventsByUserSubscriptions(long organizationId, long userId, EventsQueryRequestDTO? filterDto, long? projectId);
    Task<EventResponseDto> CreateEvent(long organizationId, long userId, CreateEventRequestDto dto, long? projectId);
    Task<EventResponseDto> BulkCreateEvents(long organizationId, List<CreateEventRequestDto> events, long? projectId);
}