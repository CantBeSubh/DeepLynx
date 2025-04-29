using System.ComponentModel.DataAnnotations;
namespace deeplynx.models;

public class ClassRequestDto
{
    [Required]
    public string Name { get; set; }
    public string? Uuid { get; set; }
    public string? Description { get; set; }
    public long ProjectId { get; set; }
}