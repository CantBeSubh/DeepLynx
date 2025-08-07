using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class CreateMetadataRequestDto
{
    public JsonArray? Classes { get; set; }
    public JsonArray? Relationships { get; set; }
    public JsonArray? Tags { get; set; }
    public JsonArray? Records { get; set; }
    public JsonArray? Edges { get; set; }
}