using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class CreateGroupRequestDto
{
    [Required]
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    public string OrganizationId { get; set; }
}