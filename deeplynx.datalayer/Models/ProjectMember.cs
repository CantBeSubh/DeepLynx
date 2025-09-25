using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("project_members", Schema = "deeplynx")]
[Index("GroupId", Name = "idx_project_members_group_id")]
[Index("Id", Name = "idx_project_members_id")]
[Index("ProjectId", Name = "idx_project_members_project_id")]
[Index("RoleId", Name = "idx_project_members_role_id")]
[Index("UserId", Name = "idx_project_members_user_id")]
[Index("ProjectId", "GroupId", "RoleId", "UserId", Name = "unique_project_member_ids", IsUnique = true)]
public partial class ProjectMember
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("role_id")]
    public long? RoleId { get; set; }

    [Column("group_id")]
    public long? GroupId { get; set; }

    [Column("user_id")]
    public long? UserId { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("ProjectMembers")]
    public virtual Group? Group { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("ProjectMembers")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("ProjectMembers")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ProjectMembers")]
    public virtual User? User { get; set; }
}
