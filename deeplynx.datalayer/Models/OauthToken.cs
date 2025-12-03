using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.datalayer.Models;

[Table("oauth_tokens", Schema = "deeplynx")]
public partial class OauthToken
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("token_hash")]
    public string TokenHash { get; set; }
    
    [Column("application_id")]
    public long? ApplicationId { get; set; }
    
    [Column("user_id")]
    public long UserId { get; set; }
    
    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }
    
    [Column("revoked")]
    public bool Revoked { get; set; }
    
    // using CreatedAt as tokens will never be updated other than for revocation, after which they cannot be restored
    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }
    
    [ForeignKey("ApplicationId")]
    [InverseProperty("OauthTokens")]
    public OauthApplication OauthApplication { get; set; } = null!;
    
    [ForeignKey("UserId")]
    [InverseProperty("OauthTokens")]
    public User User { get; set; }
}