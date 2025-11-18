using deeplynx.models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long? organizationId, long? projectId);
    Task<PaginatedResponse<EventResponseDto>> QueryAllEvents(EventsQueryRequestDTO? filterDto, long? organizationId, long? projectId);
    Task<PaginatedResponse<EventResponseDto>> QueryAuthorizedEvents(long currentUserId, EventsQueryRequestDTO? filterDto, long? organizationId, long? projectId);
    Task<List<EventResponseDto>> GetAllEventsBySubscriptions(long currentUserId, long? organizationId, long? projectId);
    Task<PaginatedResponse<EventResponseDto>> QueryEventsBySubscriptions(long currentUserId, EventsQueryRequestDTO? filterDto, long? organizationId, long? projectId);
    Task<EventResponseDto> CreateEvent(long currentUserId, CreateEventRequestDto dto, long? organizationId, long? projectId);
    Task<EventResponseDto> BulkCreateEvents(List<CreateEventRequestDto> events, long? organizationId, long? projectId);
}