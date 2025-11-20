using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long? projectId, long? organizationId);

    Task<PaginatedResponse<EventResponseDto>> QueryEvents(
        long projectId, long organizationId, EventsQueryRequestDTO? filterDto);

    Task<PaginatedResponse<EventResponseDto>> QueryEventsByUser(
        long userId, long projectId, long organizationId, EventsQueryRequestDTO? filterDto);

    Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId);

    Task<EventResponseDto> CreateEvent(
        long currentUserId, CreateEventRequestDto dto, long? projectId = null, long? organizationId = null);

    Task<List<EventResponseDto>> BulkCreateEvents(
        List<CreateEventRequestDto> events, long? projectId = null, long? organizationId = null);
}