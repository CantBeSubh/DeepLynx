using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.datalayer.Models;

/// <summary>
/// Configuration settings for project mutability controls.
/// These settings determine whether metadata operations can create new entities
/// or must validate that referenced entities already exist.
/// </summary>
public class ProjectConfig
{

    [JsonPropertyName("edgeRecordsMutable")]
    public bool EdgeRecordsMutable { get; set; } = false;
    
    [JsonPropertyName("ontologyMutable")]
    public bool OntologyMutable { get; set; } = false;

    [JsonPropertyName("tagsMutable")]
    public bool TagsMutable { get; set; } = false;
    
    public ProjectConfig() { }
    
    public ProjectConfig(bool edgeRecordsMutable, bool ontologyMutable, bool tagsMutable)
    {
        EdgeRecordsMutable = edgeRecordsMutable;
        OntologyMutable = ontologyMutable;
        TagsMutable = tagsMutable;
    }

    /// <summary>
    /// Returns the default configuration (all mutability disabled).
    /// </summary>
    public static ProjectConfig Default => new ProjectConfig(false, false, false);

    /// <summary>
    /// Returns a fully mutable configuration (legacy behavior).
    /// </summary>
    public static ProjectConfig FullyMutable => new ProjectConfig(true, true, true);
}