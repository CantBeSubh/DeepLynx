namespace deeplynx.models;

public class EventsQueryRequestDTO
{
    public long? projectId { get; set; }
    public long? organizationId { get; set; }
    public string? lastUpdatedBy { get; set; }
    public string? projectName { get; set; }
    public string? operation { get; set; }
    public string? entityType { get; set; }
    public string? entityName { get; set; }
    public string? dataSourceName { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 500;
    private const int MaxPageSize = 500;
    
    public int GetValidatedPageSize()
    {
        if (PageSize <= 0) return 25;
        return PageSize > MaxPageSize ? MaxPageSize : PageSize;
    }
}