using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("events", Schema = "deeplynx")]
[Index("DataSourceId", Name = "idx_events_data_source_id")]
[Index("Id", Name = "idx_events_id")]
[Index("ProjectId", Name = "idx_events_project_id")]
[Index("OrganizationId", Name = "idx_events_organization_id")]
public partial class Event
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("operation")]
    public string Operation { get; set; } = null!;

    [Column("entity_type")]
    public string EntityType { get; set; } = null!;

    [Column("entity_id")]
    public long? EntityId { get; set; }
    
    [Column("entity_name")]
    public string? EntityName { get; set; }

    [Column("project_id")]
    public long? ProjectId { get; set; }
    
    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("properties", TypeName = "jsonb")]
    public string Properties { get; set; } = null!;

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [ForeignKey("ProjectId")]
    [InverseProperty("Events")]
    public virtual Project? Project { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("Events")]
    public virtual Organization? Organization { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("Events")]
    public virtual DataSource? DataSource { get; set; }

    [ForeignKey("LastUpdatedBy")] 
    [InverseProperty("LastUpdatedEvents")]
    public virtual User? LastUpdatedByUser { get; set; }
}