using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[PrimaryKey("UserId", "ProjectId")]
[Table("user_projects", Schema = "deeplynx")]
[Index("ProjectId", Name = "idx_user_projects_project_id")]
[Index("RoleId", Name = "idx_user_projects_role_id")]
[Index("UserId", Name = "idx_user_projects_user_id")]
public partial class UserProject
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Key]
    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("role_id")]
    public long? RoleId { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("UserProjects")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("UserProjects")]
    public virtual Role? Role { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserProjects")]
    public virtual User User { get; set; } = null!;
}
