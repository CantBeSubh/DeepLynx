using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[PrimaryKey("OrganizationId", "UserId")]
[Table("organization_users", Schema = "deeplynx")]
[Index("OrganizationId", Name = "idx_organization_users_organization_id")]
[Index("UserId", Name = "idx_organization_users_user_id")]
[Index("OrganizationId", "UserId", Name = "unique_organization_user_ids", IsUnique = true)]
public partial class OrganizationUser
{
    [Key]
    [Column("organization_id")]
    public long OrganizationId { get; set; }

    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Column("is_org_admin")]
    public bool IsOrgAdmin { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrganizationUsers")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("OrganizationUsers")]
    public virtual User User { get; set; } = null!;
}
