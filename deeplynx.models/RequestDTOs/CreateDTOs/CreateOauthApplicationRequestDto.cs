using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class CreateOauthApplicationRequestDto
{
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    // used for oauth redirect
    [Required]
    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; }
    
    // used for any frontend/api redirect for configurable DL ecosystem apps
    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }
    
    [JsonPropertyName("app_owner_email")]
    public string? AppOwnerEmail { get; set; }
}