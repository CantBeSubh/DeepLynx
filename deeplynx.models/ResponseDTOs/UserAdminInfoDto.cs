using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class UserAdminInfoDto
{
    
    [Column("id")]
    public long Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("email")]
    public string Email { get; set; }
    [Column("username")]
    public string? Username { get; set; }
    [Column("is_sys_admin")]
    public bool IsSysAdmin { get; set; }
    [Column("is_archived")]
    public bool IsArchived { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; }
    [Column("is_org_admin")]
    public bool? IsOrgAdmin { get; set; }
    [Column("is_project_admin")]
    public bool? IsProjectAdmin { get; set; }
}