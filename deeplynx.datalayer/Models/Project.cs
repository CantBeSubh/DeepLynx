using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("projects", Schema = "deeplynx")]
[Index("Id", Name = "idx_projects_id")]
[Index("OrganizationId", Name = "idx_projects_organization_id")]
public partial class Project
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("abbreviation")]
    public string? Abbreviation { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("config", TypeName = "jsonb")]
    public string Config { get; set; } = null!;

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [Column("organization_id")]
    public long OrganizationId { get; set; }

    [InverseProperty("Project")]
    public virtual ICollection<Action> Actions { get; set; } = new List<Action>();

    [InverseProperty("Project")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("Project")]
    public virtual ICollection<DataSource> DataSources { get; set; } = new List<DataSource>();

    [InverseProperty("Project")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    [InverseProperty("Project")]
    public virtual ICollection<ObjectStorage> ObjectStorages { get; set; } = new List<ObjectStorage>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("Projects")]
    public virtual Organization Organization { get; set; }

    [InverseProperty("Project")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    [InverseProperty("Project")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Project")]
    public virtual ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();

    [InverseProperty("Project")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [InverseProperty("Project")]
    public virtual ICollection<SensitivityLabel> SensitivityLabels { get; set; } = new List<SensitivityLabel>();

    [InverseProperty("Project")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    [InverseProperty("Project")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [InverseProperty("Project")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    [InverseProperty("LastUpdatedProjects")]
    public virtual User? LastUpdatedByUser { get; set; }
    
    [InverseProperty("Project")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
