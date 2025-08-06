using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class CreateProjectRequestDto
{
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    public string? Description { get; set; }
   
    public string? Abbreviation { get; set; }
}