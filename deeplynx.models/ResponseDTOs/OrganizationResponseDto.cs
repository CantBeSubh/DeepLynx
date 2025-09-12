namespace deeplynx.models;

public class OrganizationResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public bool IsArchived { get; set; }
}