using deeplynx.models;
namespace deeplynx.interfaces;

public interface IEventBusiness
{
    Task<List<EventResponseDto>> GetAllEvents(long? projectId, long? organizationId);
    Task<PaginatedResponse<EventResponseDto>> QueryEvents(EventsQueryRequestDTO? filterDto);
    Task<PaginatedResponse<EventResponseDto>> QueryEventsByUser(EventsQueryRequestDTO? filterDto);
    Task<List<EventResponseDto>> GetAllEventsByUserProjectSubscriptions(long userId, long projectId);
    Task<EventResponseDto> CreateEvent(CreateEventRequestDto dto);
    Task<List<EventResponseDto>> BulkCreateEvents(long projectId, List<CreateEventRequestDto> events);
    Task BulkInsertEventsWithCopyAsync(
        Npgsql.NpgsqlConnection conn,
        Npgsql.NpgsqlTransaction tx,
        IReadOnlyList<CreateEventRequestDto> events,
        long projectId,
        long? userId,
        CancellationToken ct = default);
}