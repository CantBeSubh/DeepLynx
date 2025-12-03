using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class OauthApplicationSecureResponseDto
{
    public string Name { get; set; }
    
    public string ClientId { get; set; }
    
    public string ClientSecretRaw { get; set; }
}