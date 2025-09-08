namespace deeplynx.models;

public class OrganizationResponseDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
}