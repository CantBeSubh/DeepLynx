using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class ProjectRequestDto
{
    [Required]
    public string Name { get; set; }
    
   
    public string? Abbreviation { get; set; }
    
}