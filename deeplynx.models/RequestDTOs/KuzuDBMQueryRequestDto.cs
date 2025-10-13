namespace deeplynx.models
{
    using System.ComponentModel.DataAnnotations;

    public class KuzuDBMQueryRequestDto
    {
        [Required]
        public string Query { get; set; }
    }
}
