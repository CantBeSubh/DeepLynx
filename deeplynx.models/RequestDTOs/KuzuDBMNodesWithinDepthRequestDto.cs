namespace deeplynx.models
{
    using System.ComponentModel.DataAnnotations;

    public class KuzuDBMNodesWithinDepthRequestDto
    {
        [Required]
        public string TableName { get; set; }
        [Required]
        public long Id { get; set; }
        [Required]
        public int Depth { get; set; }
    }
}
