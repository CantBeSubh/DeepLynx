namespace deeplynx.models;

public class UpdateSubscriptionRequestDto
{
    public long Id { get; set; }
    public long ActionId {get; set;}
    public string Operation {get; set;}
    public long DataSourceId {get; set;}
    public string EntityType {get; set;}
    public  long EntityId {get; set;}
}