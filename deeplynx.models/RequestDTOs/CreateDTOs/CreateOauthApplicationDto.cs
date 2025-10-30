using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class CreateOauthApplicationDto
{
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [Required]
    [JsonPropertyName("redirect_uris")]
    public List<string> RedirectUris { get; set; }
    
    [JsonPropertyName("app_owner_email")]
    public string? AppOwnerEmail { get; set; }
}