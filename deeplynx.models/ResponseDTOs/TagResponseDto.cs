namespace deeplynx.models;

public class TagResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long ProjectId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}