using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("permissions", Schema = "deeplynx")]
[Index("Action", Name = "idx_permissions_action")]
[Index("Domain", Name = "idx_permissions_domain")]
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

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("LabelId")]
    [InverseProperty("Permissions")]
    public virtual SensitivityLabel? Label { get; set; }

    [ForeignKey("PermissionId")]
    [InverseProperty("Permissions")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
