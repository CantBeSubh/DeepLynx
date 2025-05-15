namespace deeplynx.models;

public class RelationshipResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Uuid { get; set; }
    public long ProjectId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public long OriginId { get; set; }
    public long DestinationId { get; set; }
    public ClassResponseDto? Origin { get; set; }
    public ClassResponseDto? Destination { get; set; }
}
public class ClassResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}
