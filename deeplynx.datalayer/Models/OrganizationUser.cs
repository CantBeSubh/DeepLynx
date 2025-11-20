using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.datalayer.Models;

[Table("organization_users", Schema = "deeplynx")]
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
