using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("users", Schema = "deeplynx")]
[Index("Id", Name = "idx_users_id")]
[Index("Email", Name = "idx_users_email")]
public partial class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; }
    
    [Column("password")]
    public string? Password { get; set; }
    [Required]
    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;

    [Column("is_sysadmin")]
    public bool IsSysAdmin { get; set; } = false;
    
    [InverseProperty("Users")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    
    [InverseProperty("User")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    [InverseProperty("User")]
    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();
    
    [InverseProperty("User")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    
    [InverseProperty("Users")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}