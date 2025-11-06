using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class CreateProjectRequestDto
{
    [Required]
    public string Name { get; set; }
    
<<<<<<< Updated upstream
=======
    public long? OrganizationId { get; set; }

>>>>>>> Stashed changes
    public string? Description { get; set; }
   
    public string? Abbreviation { get; set; }
    
    public long? OrganizationId { get; set; }
}