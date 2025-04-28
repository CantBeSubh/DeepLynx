using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("role_permissions", Schema = "deeplynx")]
[Index("ActionType", Name = "idx_role_permissions_action_type")]
[Index("PermissionId", Name = "idx_role_permissions_permission_id")]
[Index("RoleId", Name = "idx_role_permissions_role_id")]
public partial class RolePermission
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("role_id")]
    public long RoleId { get; set; }

    [Column("permission_id")]
    public long PermissionId { get; set; }

    [Column("action_type")]
    public string ActionType { get; set; } = null!;

    [ForeignKey("PermissionId")]
    [InverseProperty("RolePermissions")]
    public virtual Permission Permission { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("RolePermissions")]
    public virtual Role Role { get; set; } = null!;
}
