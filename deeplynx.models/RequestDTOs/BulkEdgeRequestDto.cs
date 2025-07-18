using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class BulkEdgeRequestDto
{
    [Required]
    public List<EdgeRequestDto> Edges { get; set; }
}