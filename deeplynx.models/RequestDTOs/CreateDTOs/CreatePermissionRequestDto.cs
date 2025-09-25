using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

// Only allow users to create label-based permissions
public class CreatePermissionRequestDto
{
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; }
    [Required]
    public string Action { get; set; }
    [Required]
    public long LabelId { get; set; }
    public long? ProjectId { get; set; }
    public long? OrganizationId { get; set; }
}