using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace deeplynx.models;

public class ProjectDto
{
    [Required]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Abbreviation { get; set; }
    
    public string? CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    [Required]
    public DateTime ModifiedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
}