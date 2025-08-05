using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class CreateRecordMappingRequestDto
{
    [Required]
    public JsonObject RecordParams { get; set; }
    
    public long DataSourceId { get; set; }
    
    public long? ClassId { get; set; }
    
    public long? TagId { get; set; }
}