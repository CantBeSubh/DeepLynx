using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.datalayer.Models;

[Table("saved_searches", Schema = "deeplynx")]
public class SavedSearch
{
    [Key] [Column("id")] public long Id { get; set; }

    [Column("name")] public string Name { get; set; }

    [Column("search", TypeName = "jsonb")] public string Search { get; set; } = null!;

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_favorite")] public bool IsFavorite { get; set; } 

    [Column("favorite_order")]
    [Range(1, 10)]
    public int? FavoriteOrder { get; set; }
    
    [Column("user_id")]
    public long? UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("SavedSearches")]
    public virtual User User { get; set; } = null!;
    
}