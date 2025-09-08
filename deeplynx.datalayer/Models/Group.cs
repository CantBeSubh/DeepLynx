using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("groups", Schema = "deeplynx")]
[Index("Id", Name = "idx_groups_id")]
[Index("OrganizationId", Name = "idx_groups_organization_id")]
public partial class Group
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
    
    [Column("organization_id")]
    public long OrganizationId { get; set; }
    
    [ForeignKey("OrganizationId")]
    [InverseProperty("Groups")]
    public virtual Organization Organization { get; set; } = null!;
    
    [InverseProperty("Group")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    
    [InverseProperty("Groups")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}