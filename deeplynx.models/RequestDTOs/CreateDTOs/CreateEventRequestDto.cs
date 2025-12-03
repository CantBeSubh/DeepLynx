namespace deeplynx.models
{
    public class CreateEventRequestDto
    {
        public string Operation { get; set; }
        public string EntityType { get; set; }
        public long? EntityId { get; set; }
        public string? EntityName {get; set;}
        public long? DataSourceId { get; set; }
        public string Properties { get; set; }
        public long? LastUpdatedBy { get; set; }
    }
}