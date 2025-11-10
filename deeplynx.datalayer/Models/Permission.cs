using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("permissions", Schema = "deeplynx")]
[Index("Action", Name = "idx_permissions_action")]
[Index("Resource", Name = "idx_permissions_resource")]
[Index("Id", Name = "idx_permissions_id")]
[Index("LabelId", Name = "idx_permissions_label_id")]
[Index("ProjectId", Name = "idx_permissions_project_id")]
[Index("OrganizationId", Name = "idx_permissions_organization_id")]
[Index("IsDefault", Name = "idx_permissions_is_default")]
[Index("ProjectId", "OrganizationId", "LabelId", "Action", Name = "permissions_unique_label_action", IsUnique = true)]
[Index("ProjectId", "Resource", "Action", Name = "permissions_unique_project_resource_action", IsUnique = true)]
[Index("OrganizationId", "Resource", "Action", Name = "permissions_unique_org_resource_action", IsUnique = true)]
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

    [Column("resource")]
    public string? Resource { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("label_id")]
    public long? LabelId { get; set; }

    [Column("project_id")]
    public long? ProjectId { get; set; }

    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("LabelId")]
    [InverseProperty("Permissions")]
    public virtual SensitivityLabel? Label { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Permissions")]
    public virtual Project? Project { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("Permissions")]
    public virtual Organization? Organization { get; set; }

    [ForeignKey("PermissionId")]
    [InverseProperty("Permissions")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    
    [InverseProperty("LastUpdatedPermissions")]
    public virtual User? LastUpdatedByUser { get; set; }
}
