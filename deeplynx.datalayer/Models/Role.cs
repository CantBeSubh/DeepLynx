using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("roles", Schema = "deeplynx")]
[Index("Id", Name = "idx_roles_id")]
[Index("ProjectId", Name = "idx_roles_project_id")]
public partial class Role
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")] 
    public string Name { get; set; } = null!;
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }
    
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [Column("project_id")]
    public long ProjectId { get; set; }
    
    [ForeignKey("ProjectId")]
    [InverseProperty("Roles")]
    public virtual Project Project { get; set; } = null!;
    
    [InverseProperty("Role")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
}