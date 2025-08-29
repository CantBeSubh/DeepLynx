using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models
{
    [Table("subscriptions", Schema = "deeplynx")]
    [Index("Id", Name = "idx_subscriptions_id")]
    [Index("UserId", Name = "idx_subscriptions_user_id")]
    [Index("ProjectId", Name = "idx_subscriptions_project_id")]
    [Index("EntityType", Name = "idx_subscriptions_entity_type")]
    public partial class Subscription
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Column("user_id")] 
        public long? UserId { get; set; }
        
        [Column("action_id")] 
        public long ActionId { get; set; }
        
        [Column("operation")]  
        public string? Operation { get; set; }
        
        [Column("project_id")] 
        public long ProjectId { get; set; }
        
        [Column("data_source_id")] 
        public long? DataSourceId { get; set; }
        
        [Column("entity_type")] 
        public string? EntityType { get; set; }
        
        [Column("entity_id")] 
        public long? EntityId { get; set; }
        
        [Required]
        [Column("last_updated_at", TypeName = "timestamp without time zone")]
        public DateTime LastUpdatedAt { get; set; }
    
        [Column("last_updated_by")]
        public string? LastUpdatedBy { get; set; }
    
        [Required]
        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;
        [ForeignKey("UserId")]
        [InverseProperty("Subscriptions")]
        public virtual User? User { get; set; }
        
        [ForeignKey("ActionId")]
        [InverseProperty("Subscriptions")]
        public virtual Action? Action { get; set; }

        [ForeignKey("ProjectId")]
        [InverseProperty("Subscriptions")]
        public virtual Project? Project { get; set; }

        [ForeignKey("DataSourceId")]
        [InverseProperty("Subscriptions")]
        public virtual DataSource? DataSource { get; set; }
    }
}
