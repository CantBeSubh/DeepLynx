using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
    
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }
    
    [Required]
    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;
    
    [Column("config", TypeName = "jsonb")]
    public string ConfigJson { get; set; } = null!;
    
    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    /// <summary>
    /// Strongly-typed access to project configuration.
    /// Automatically serializes/deserializes from the ConfigJson column.
    /// </summary>
    [NotMapped]
    public ProjectConfig Config
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ConfigJson))
                return ProjectConfig.Default;

            try
            {
                return JsonSerializer.Deserialize<ProjectConfig>(ConfigJson) ?? ProjectConfig.Default;
            }
            catch (JsonException)
            {
                // If JSON is invalid, return default config
                return ProjectConfig.Default;
            }
        }
        set
        {
            ConfigJson = JsonSerializer.Serialize(value);
        }
    }

    [InverseProperty("Project")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("Project")]
    public virtual ICollection<DataSource> DataSources { get; set; } = new List<DataSource>();

    [InverseProperty("Project")]
    public virtual ICollection<EdgeMapping> EdgeMappings { get; set; } = new List<EdgeMapping>();

    [InverseProperty("Project")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    [InverseProperty("Project")]
    public virtual ICollection<RecordMapping> RecordMappings { get; set; } = new List<RecordMapping>();

    [InverseProperty("Project")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Project")]
    public virtual ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();

    [InverseProperty("Project")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    
    [InverseProperty("Project")]
    public virtual ICollection<ObjectStorage> ObjectStorages { get; set; } = new List<ObjectStorage>();
    
    [InverseProperty("Projects")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    [InverseProperty("Project")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    
    [InverseProperty("Project")]
    public virtual ICollection<Action> Actions { get; set; } = new List<Action>();
    
    [InverseProperty("Project")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    [ForeignKey("OrganizationId")]
    [InverseProperty("Projects")]
    public virtual Organization? Organization { get; set; } = null!;
    
    [InverseProperty("Project")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [InverseProperty("Project")]
    public virtual ICollection<SensitivityLabel> SensitivityLabels { get; set; } = new List<SensitivityLabel>();
    
    [InverseProperty("Project")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
}
