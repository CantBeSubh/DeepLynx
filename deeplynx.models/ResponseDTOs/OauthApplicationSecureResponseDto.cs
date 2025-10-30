using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class OauthApplicationSecureResponseDto
{
    [Column("name")]
    public string Name { get; set; }
    
    [Column("client_id")]
    public string ClientId { get; set; }
    
    [Column("client_secret_raw")]
    public string ClientSecretRaw { get; set; }
}