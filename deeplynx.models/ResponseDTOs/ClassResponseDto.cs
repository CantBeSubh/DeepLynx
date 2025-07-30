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
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
    [Column("archived_at")]
    public DateTime? ArchivedAt { get; set; }
}