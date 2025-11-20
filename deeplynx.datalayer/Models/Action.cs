using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.datalayer.Models;

[Table("actions", Schema = "deeplynx")]
public partial class Action
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }
    
    [Column("organization_id")]
    public long OrganizationId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("config", TypeName = "jsonb")]
    public string? Config { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Actions")]
    public virtual Project Project { get; set; } = null!;
    
    [ForeignKey("OrganizationId")]
    [InverseProperty("Actions")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Action")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    [InverseProperty("LastUpdatedActions")]
    public virtual User? LastUpdatedByUser { get; set; }
}
