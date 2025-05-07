using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class RecordMappingRequestDto
{
    [Required]
    public JsonObject RecordParams { get; set; }
    
    public int? ClassId { get; set; }
    
    public int? TagId { get; set; }
}