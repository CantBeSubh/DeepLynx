using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class RecordTagLinkDto
{
    [JsonPropertyName("record_id")]
    public long RecordId { get; set; } 
    
    [JsonPropertyName("tag_id")]
    public long TagId { get; set; }
}