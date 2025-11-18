using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("subscriptions", Schema = "deeplynx")]
[Index("ActionId", Name = "IX_subscriptions_action_id")]
[Index("DataSourceId", Name = "IX_subscriptions_data_source_id")]
[Index("EntityType", Name = "idx_subscriptions_entity_type")]
[Index("Id", Name = "idx_subscriptions_id")]
[Index("OrganizationId", Name = "idx_subscriptions_organization_id")]
[Index("ProjectId", Name = "idx_subscriptions_project_id")]
[Index("UserId", Name = "idx_subscriptions_user_id")]
public partial class Subscription
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("action_id")]
    public long ActionId { get; set; }

    [Column("operation")]
    public string? Operation { get; set; }
    
    // REQUIRED: OrganizationId cannot be null
    [Required]
    [Column("organization_id")]
    public long OrganizationId { get; set; }

    // OPTIONAL: ProjectId can be null
    [Column("project_id")]
    public long? ProjectId { get; set; }

    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("entity_type")]
    public string? EntityType { get; set; }

    [Column("entity_id")]
    public long? EntityId { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [ForeignKey("ActionId")]
    [InverseProperty("Subscriptions")]
    public virtual Action Action { get; set; } = null!;

    [ForeignKey("DataSourceId")]
    [InverseProperty("Subscriptions")]
    public virtual DataSource? DataSource { get; set; }

    [Required]
    [ForeignKey("OrganizationId")]
    [InverseProperty("Subscriptions")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("Subscriptions")]
    public virtual Project? Project { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Subscriptions")]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey("LastUpdatedBy")]
    [InverseProperty("LastUpdatedSubscriptions")]
    public virtual User? LastUpdatedByUser { get; set; }
}