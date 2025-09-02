using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("permissions", Schema = "deeplynx")]
[Index("Id", Name = "idx_permissions_id")]
[Index("LabelId", Name = "idx_permissions_label_id")]
public partial class Permission
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")] 
    public string Name { get; set; } = null!;
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("action")]
    public string Action { get; set; } = null!;
    
    [Column("domain")]
    public string? Domain { get; set; }
    
    [Column("label_id")]
    public long? LabelId { get; set; }
    
    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }
    
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [InverseProperty("Permissions")]
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    
    [ForeignKey("LabelId")]
    [InverseProperty("Permissions")]
    public virtual SensitivityLabel? SensitivityLabel { get; set; }
}