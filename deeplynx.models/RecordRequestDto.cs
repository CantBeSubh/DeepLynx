using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace deeplynx.models;

public class RecordRequestDto
{
    [Required]
    public JsonElement Properties { get; set; }

    public int? Depth { get; set; }

    public object? Name { get; set; }

    public object? original_id { get; set; }

    public string? ClassName { get; set; }
}