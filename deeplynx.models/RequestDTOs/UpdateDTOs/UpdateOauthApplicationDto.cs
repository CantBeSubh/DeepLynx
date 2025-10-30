using System.Text.Json.Serialization;

namespace deeplynx.models;

public class UpdateOauthApplicationDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("redirect_uris")]
    public List<string>? RedirectUris { get; set; }
    
    [JsonPropertyName("app_owner_email")]
    public string? AppOwnerEmail { get; set; }
}