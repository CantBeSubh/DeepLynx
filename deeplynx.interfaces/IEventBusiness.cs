using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long? organizationId, long? projectId);

    Task<PaginatedResponse<EventResponseDto>> QueryAllEvents(EventsQueryRequestDTO? queryDto,
        long? organizationId, long? projectId);

    Task<PaginatedResponse<EventResponseDto>> QueryAuthorizedEvents(long currentUserId,
        EventsQueryRequestDTO? queryDto, long? organizationId, long? projectId);
    
    Task<PaginatedResponse<EventResponseDto>> QueryEventsBySubscriptions(long currentUserId,
        EventsQueryRequestDTO? queryDto, long? organizationId, long? projectId);

    Task<EventResponseDto> CreateEvent(long currentUserId, CreateEventRequestDto dto, long? organizationId,
        long? projectId);

    Task<EventResponseDto> BulkCreateEvents(
        long currentUserId,
        List<CreateEventRequestDto> events,
        long? organizationId,
        long? projectId = null
    );
}