namespace deeplynx.models
{
    public class DataSourceDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public long ProjectId { get; set; }
        public string? Abbreviation { get; set; }
        public string? Type { get; set; }
        public string? BaseUri { get; set; }
        public string? Config { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}