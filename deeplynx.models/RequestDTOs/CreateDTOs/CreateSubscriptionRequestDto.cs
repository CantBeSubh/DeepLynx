namespace deeplynx.models;

public class CreateSubscriptionRequestDto
{
    public long UserId { get; set; }
    public long ProjectId { get; set; }
    public long ActionId { get; set; }
    public string? Operation { get; set; }
    public long? DataSourceId { get; set; }
    public string? EntityType {get; set;}
    public long? EntityId { get; set; }
}