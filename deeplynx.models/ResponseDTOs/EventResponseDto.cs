using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

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
    public long ProjectId { get; set; }
    
    [Column("data_source_id")]
    public long? DataSourceId { get; set; }
    
    [Column("properties")]
    public string Properties { get; set; }
    
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}