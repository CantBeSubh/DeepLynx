using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class ProjectRequestDto
{
    [Required]
    public string Name { get; set; }
    
   
    public string? Abbreviation { get; set; }
    
}