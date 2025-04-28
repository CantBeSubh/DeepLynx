namespace deeplynx.models
{
    public class DataSource
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public long ProjectId { get; set; }
    }
}