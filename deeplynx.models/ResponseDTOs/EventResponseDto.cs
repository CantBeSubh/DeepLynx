using System.ComponentModel.DataAnnotations.Schema;

public class EventResponseDto
{
    [Column("id")]
    public long Id { get; set; }

    [Column("operation")]
    public string Operation { get; set; }

    [Column("entity_type")]
    public string EntityType { get; set; }

    [Column("entity_id")]
    public long? EntityId { get; set; }

    [Column("project_id")]
    public long? ProjectId { get; set; }

    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("properties")]
    public string Properties { get; set; }
    public string? ProjectName { get; set; }
    public string? EntityName { get; set; }
    public string? DataSourceName { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public long? LastUpdatedBy { get; set; }
    public string? LastUpdatedByUserName { get; set; }
}
