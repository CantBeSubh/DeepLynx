using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("organization_users", Schema = "deeplynx")]
[Index("OrganizationId", Name = "idx_organization_users_organization_id")]
[Index("UserId", Name = "idx_organization_users_user_id")]
[Index(nameof(OrganizationId), nameof(UserId), IsUnique = true, Name = "unique_organization_user_ids")]
public partial class OrganizationUser
{
    [Column("organization_id")]
    public long OrganizationId { get; set; }
    
    [Column("user_id")]
    public long UserId { get; set; }

    [Column("is_org_admin")]
    public bool IsOrgAdmin { get; set; } = false;
    
    [ForeignKey("OrganizationId")]
    [InverseProperty("OrganizationUsers")]
    public virtual Organization Organization { get; set; }
    
    [ForeignKey("UserId")]
    [InverseProperty("OrganizationUsers")]
    public virtual User User { get; set; }
}