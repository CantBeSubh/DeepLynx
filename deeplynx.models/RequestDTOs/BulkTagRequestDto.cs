using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class BulkTagRequestDto
{
    [Required]
    public List<TagRequestDto> Tags { get; set; }
}