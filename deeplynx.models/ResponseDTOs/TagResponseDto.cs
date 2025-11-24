using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class TagResponseDto
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("project_id")]
    public long? ProjectId { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;

    [Column("organization_id")]
    public long OrganizationId { get; set; }
}