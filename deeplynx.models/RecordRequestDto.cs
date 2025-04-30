using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class RecordRequestDto
{
    
    public string? Uri { get; set; }
    
    [Required]
    public JsonObject Properties { get; set; }
    
    public string? OriginalId { get; set; }
    public string? Name { get; set; }

    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    
}