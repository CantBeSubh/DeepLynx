using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class BulkRecordRequestDto
{
    [Required]
    public List<RecordRequestDto> Records { get; set; }
}