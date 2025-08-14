namespace deeplynx.models;

public class ObjectStorageResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Default { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}