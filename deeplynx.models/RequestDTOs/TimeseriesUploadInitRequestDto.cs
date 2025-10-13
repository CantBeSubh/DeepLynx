using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class TimeseriesUploadInitRequestDto
{
    [Required]
    public string FileName { get; set; }

    public long FileSize { get; set; }
}
