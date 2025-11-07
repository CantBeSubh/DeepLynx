using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class CreateProjectRequestDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public long OrganizationId { get; set; }

    public string? Description { get; set; }

    public string? Abbreviation { get; set; }
}