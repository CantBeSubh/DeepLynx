using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models
{
    [Table("actions", Schema = "deeplynx")]
    [Index("Id", Name = "idx_actions_id")]
    public partial class Action
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Column("project_id")] 
        public long ProjectId { get; set; }

        [Column("name")] 
        public string Name { get; set; }
        
        [Column("config", TypeName = "jsonb")]  
        public string? Config { get; set; }
        
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
        
        [ForeignKey("ProjectId")]
        [InverseProperty("Actions")]
        public virtual Project? Project { get; set; }
        
        [InverseProperty("Action")]
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}