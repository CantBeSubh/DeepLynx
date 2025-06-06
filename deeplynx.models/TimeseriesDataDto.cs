using System.ComponentModel.DataAnnotations;
namespace deeplynx.models;

public class TimeseriesDataDto
{
    public string ProjectId { get; set; }
    public string DataSourceId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; } // e.g., "csv" or "tdms"
}