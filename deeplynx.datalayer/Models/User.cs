using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("users", Schema = "deeplynx")]
[Index("Email", Name = "idx_users_email", IsUnique = true)]
[Index("Id", Name = "idx_users_id")]
[Index("SsoId", Name = "idx_users_sso_id")]
public partial class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;
    
    [Column("username")]
    public string? Username { get; set; } = null!;
    
    [Column("sso_id")]
    public string? SsoId { get; set; }

    [Column("is_active")] 
    public bool IsActive { get; set; }

    [Column("password")]
    public string? Password { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [Column("is_sys_admin")]
    public bool IsSysAdmin { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();

    [InverseProperty("User")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    [InverseProperty("User")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    
    [InverseProperty("User")]
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    [InverseProperty("LastUpdatedByUser")]
    public virtual ICollection<Class> UpdatedClasses { get; set; } = new List<Class>();

    
    [InverseProperty("User")]
    public virtual ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
}
