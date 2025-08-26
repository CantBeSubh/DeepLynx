using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class ClassResponseDto
{
    [Column("id")]
    public long Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
    [Column("description")]
    public string? Description { get; set; }
    [Column("uuid")]
    public string? Uuid { get; set; }
    [Column("project_id")]
    public long ProjectId { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public bool IsArchived { get; set; }
}