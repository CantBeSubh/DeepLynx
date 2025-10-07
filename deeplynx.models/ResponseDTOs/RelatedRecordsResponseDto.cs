namespace deeplynx.models;

public class RelatedRecordsResponseDto
{
    public string RelatedRecordName { get; set; }
    public long RelatedRecordId { get; set; }
    public long RelatedRecordProjectId { get; set; }
    public string? RelationshipName { get; set; }
    public bool IsOrigin { get; set; }
}