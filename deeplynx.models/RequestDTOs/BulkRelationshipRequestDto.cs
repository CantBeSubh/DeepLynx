using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class BulkRelationshipRequestDto
{
    [Required]
    public List<RelationshipRequestDto> Relationships { get; set; }
}