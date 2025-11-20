using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("api_keys", Schema = "deeplynx")]
[Index("ApplicationId", Name = "idx_api_keys_application_id")]
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
    
    [Column("application_id")]
    public long? ApplicationId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ApiKeys")]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey("ApplicationId")]
    [InverseProperty("ApiKeys")]
    public virtual OauthApplication? OauthApplication { get; set; }
}