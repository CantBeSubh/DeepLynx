namespace deeplynx.models;
using System.ComponentModel.DataAnnotations;

public class TimeseriesQueryRequestDto
{
    [Required]
    public string Query { get; set; }
}