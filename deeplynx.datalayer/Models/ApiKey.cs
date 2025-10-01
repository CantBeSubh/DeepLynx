using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.datalayer.Models;

[Table("api_keys", Schema = "deeplynx")]
public partial class ApiKey
{
    
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }
    [Column("secret")]
    public string Secret { get; set; }
    [Column("key")]
    public string Key { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ApiKeys")]
    public virtual User User { get; set; } = null!;
}