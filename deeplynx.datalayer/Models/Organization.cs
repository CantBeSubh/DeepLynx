using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("organizations", Schema = "deeplynx")]
[Index("Id", Name = "idx_organizations_id")]
[Index("Name", Name = "unique_organization_name", IsUnique = true)]
public partial class Organization
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();

    [InverseProperty("Organization")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    [InverseProperty("Organization")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [InverseProperty("Organization")]
    public virtual ICollection<SensitivityLabel> SensitivityLabels { get; set; } = new List<SensitivityLabel>();

    [InverseProperty("Organization")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [InverseProperty("LastUpdatedOrganizations")]
    public virtual User? LastUpdatedByUser { get; set; }
}
