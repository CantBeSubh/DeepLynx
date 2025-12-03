using System.Text.Json.Serialization;

namespace deeplynx.models;

public class UpdateOauthApplicationRequestDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    // used for oauth redirect
    public string? CallbackUrl { get; set; }
    
    // used for any frontend/api redirect for configurable DL ecosystem apps
    public string? BaseUrl { get; set; }
    
    public string? AppOwnerEmail { get; set; }
}