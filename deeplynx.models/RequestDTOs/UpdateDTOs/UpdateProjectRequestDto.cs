using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class UpdateProjectRequestDto
{
    [Required]
    public long OrganizationId { get; set; }
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Abbreviation { get; set; }
}