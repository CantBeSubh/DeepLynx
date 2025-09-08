using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("sensitivity_labels", Schema = "deeplynx")]
[Index("Id", Name = "idx_sensitivity_labels_id")]
[Index("Name", Name = "idx_sensitivity_labels_name")]
[Index("ProjectId", Name = "idx_sensitivity_labels_project_id")]
[Index("OrganizationId", Name = "idx_sensitivity_labels_organization_id")]
public partial class SensitivityLabel
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
    [InverseProperty("SensitivityLabels")]
    public virtual Project? Project { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("SensitivityLabels")]
    public virtual Organization? Organization { get; set; } = null!;
    
    [InverseProperty("SensitivityLabel")]
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    
    [ForeignKey("LabelId")]
    [InverseProperty("SensitivityLabels")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}