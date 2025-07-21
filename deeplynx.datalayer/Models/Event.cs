using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models
{
    [Table("events", Schema = "deeplynx")]
    [Index("Id", Name = "idx_events_id")]
    [Index("UserId", Name = "idx_events_user_id")]
    [Index("ProjectId", Name = "idx_events_project_id")]
    public partial class Event
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [Column("operation")]
        public string Operation { get; set; }

        [Column("entity_type")]
        public string EntityType { get; set; }

        [Column("entity_id")]
        public long? EntityId { get; set; }

        [Column("project_id")]
        public long ProjectId { get; set; }

        [Column("data_source_id")]
        public long? DataSourceId { get; set; }

        [Column("properties", TypeName = "jsonb")]
        public string Properties { get; set; }
        
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_at", TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_at", TypeName = "timestamp without time zone")]
        public DateTime? ModifiedAt { get; set; }

        [Column("archived_at", TypeName = "timestamp without time zone")]
        public DateTime? ArchivedAt { get; set; }
        
        [ForeignKey("UserId")]
        [InverseProperty("Events")]
        public virtual User? User { get; set; }

        [ForeignKey("ProjectId")]
        [InverseProperty("Events")]
        public virtual Project? Project { get; set; }

        [ForeignKey("DataSourceId")]
        [InverseProperty("Events")]
        public virtual DataSource? DataSource { get; set; }
    }
}