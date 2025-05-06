using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class RecordParameterRequestDto
{
    [Required]
    public JsonObject RecordParams { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    public int? ClassId { get; set; }
    
    public int? TagId { get; set; }
}