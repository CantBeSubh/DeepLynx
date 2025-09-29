namespace deeplynx.models;

// only allow users to update label-based permissions
public class UpdatePermissionRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Action { get; set; }
    public long? LabelId { get; set; }
}