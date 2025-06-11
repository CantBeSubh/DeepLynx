using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class TimeseriesUploadCompleteRequestDto
{
    [Required]
    public string UploadId { get; set; }

    [Required]
    public string FileName { get; set; }

    public int TotalChunks { get; set; }
}