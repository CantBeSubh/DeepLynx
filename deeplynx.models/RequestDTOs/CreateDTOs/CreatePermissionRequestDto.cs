namespace deeplynx.models;

// Only allow users to create label-based permissions
public class CreatePermissionRequestDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Action { get; set; }
    public long LabelId { get; set; }
    public long? ProjectId { get; set; }
    public long? OrganizationId { get; set; }
}