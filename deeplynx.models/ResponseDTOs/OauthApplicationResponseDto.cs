using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class OauthApplicationResponseDto
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("client_id")]
    public string ClientId { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("callback_url")]
    public string CallbackUrl { get; set; }
    
    [Column("base_url")]
    public string? BaseUrl { get; set; }
    
    [Column("app_owner_email")]
    public string? AppOwnerEmail { get; set; }
    
    [Column("is_archived")]
    public bool IsArchived { get; set; }
    
    [Column("last_updated_at")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }
}