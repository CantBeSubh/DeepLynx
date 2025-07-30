using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class TagResponseDto
{
    [Column("id")]
    public long Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
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