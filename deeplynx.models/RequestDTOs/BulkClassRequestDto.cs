using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class BulkClassRequestDto
{
    [Required]
    public List<ClassRequestDto> BulkClassRequests { get; set; }
}