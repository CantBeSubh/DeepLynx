namespace deeplynx.models;

public class SubscriptionResponseDto
{
    public long Id { get; set; }
    public long UserId {get; set;}
    public long? OrganizationId {get; set;}
    public long? ProjectId {get; set;}
    public long ActionId {get; set;}
    public string? Operation {get; set;}
    public long? DataSourceId {get; set;}
    public string? EntityType {get; set;}
    public long? EntityId {get; set;}
   
    public DateTime? LastUpdatedAt { get; set; }
    
    public long? LastUpdatedBy { get; set; }
  
    public bool IsArchived { get; set; } = false;
}