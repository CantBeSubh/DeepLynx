namespace deeplynx.models;


public class FullTextQueryResponseDto
{
    public string? Uri { get; set; }
    public string Properties { get; set; } = null!;
    public string? OriginalId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ClassName { get; set; }
    public string? DataSourceName { get; set; }
    public string? ProjectName { get; set; }
    public string? Tags { get; set; } = null!;
    
}