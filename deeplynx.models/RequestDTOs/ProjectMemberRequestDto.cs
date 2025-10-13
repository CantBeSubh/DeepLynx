using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class ProjectMemberRequestDto
{
    [Required]
    public long ProjectId { get; set; }
    
    [Required]
    public long RoleId { get; set; }
    
    public long? UserId { get; set; }
    public long? GroupId { get; set; }
}