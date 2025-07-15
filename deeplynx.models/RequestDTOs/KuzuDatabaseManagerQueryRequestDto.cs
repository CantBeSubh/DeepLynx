namespace deeplynx.models
{
    using System.ComponentModel.DataAnnotations;

    public class KuzuDatabaseManagerQueryRequestDto
    {
        [Required]
        public string Query { get; set; }

        public int? ProjectId { get; set; }
    }
}