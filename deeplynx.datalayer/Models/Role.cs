using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("roles", Schema = "deeplynx")]
[Index("Id", Name = "idx_roles_id")]
[Index("ProjectId", Name = "idx_roles_project_id")]
[Index("OrganizationId", Name = "idx_roles_organization_id")]
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
    public long? ProjectId { get; set; }

    [Column("organization_id")]
    public long? OrganizationId { get; set; }
    
    [ForeignKey("ProjectId")]
    [InverseProperty("Roles")]
    public virtual Project? Project { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("Roles")]
    public virtual Organization? Organization { get; set; } = null!;
    
    [InverseProperty("Role")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    
    [InverseProperty("Roles")]
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}