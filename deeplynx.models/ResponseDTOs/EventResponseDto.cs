using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class EventResponseDto
{
    public long Id { get; set; }
    
    public string Operation { get; set; }
    
    public string EntityType { get; set; }
    
    public long? EntityId { get; set; }
    
    public long ProjectId { get; set; }
    
    public long? DataSourceId { get; set; }
    
    public string Properties { get; set; }
    
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }

}