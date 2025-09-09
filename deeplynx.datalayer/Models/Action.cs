using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models
{
    [Table("actions", Schema = "deeplynx")]
    [Index("Id", Name = "idx_actions_id")]
    [Index("ProjectId", Name = "idx_project_id")]
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
        
        [Required]
        [Column("last_updated_at", TypeName = "timestamp without time zone")]
        public DateTime LastUpdatedAt { get; set; }
    
        [Column("last_updated_by")]
        public string? LastUpdatedBy { get; set; }
    
        [Required]
        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;
        
        [ForeignKey("ProjectId")]
        [InverseProperty("Actions")]
        public virtual Project? Project { get; set; }
        
        [InverseProperty("Action")]
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}