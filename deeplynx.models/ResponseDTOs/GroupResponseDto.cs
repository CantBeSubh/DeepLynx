namespace deeplynx.models;

public class GroupResponseDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public long? OrganizationId { get; set; }
}