using System;
using System.Text;

namespace deeplynx.models
{
    public class KuzuDBMResponseDto
    {
        public long? RecordId { get; set; }
        public string? InternalId { get; set; }
        public string? Label { get; set; }
        public long? Id { get; set; }
        public long? ProjectId { get; set; }
        public string? ClassName { get; set; }
        public string? ProjectName { get; set; }
        public string? Name { get; set; }
        public string? Uri { get; set; }
        public long? DataSourceId { get; set; }
        public string? OriginalId { get; set; }
        public long? ClassId { get; set; }
        public string? Properties { get; set; }
        public string? Tags { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }


        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine("{");
            sb.AppendLine($"_ID: {InternalId ?? "NULL"}");
            sb.AppendLine($"_LABEL: {Label ?? "NULL"},");
            sb.AppendLine($"project_id: {ProjectId?.ToString() ?? "NULL"},");
            sb.AppendLine($"last_updated_at: {LastUpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss.fffffff") ?? "NULL"},");
            sb.AppendLine($"id: {Id?.ToString() ?? "NULL"},");
            sb.AppendLine($"class_name: {ClassName ?? "NULL"},");
            sb.AppendLine($"uri: {Uri ?? "NULL"},");
            sb.AppendLine($"record_id: {RecordId?.ToString() ?? "NULL"},");
            sb.AppendLine($"properties: {Properties ?? "{}"},");
            sb.AppendLine($"data_source_id: {DataSourceId?.ToString() ?? "NULL"},");
            sb.AppendLine($"original_id: {OriginalId ?? "NULL"},");
            sb.AppendLine($"class_id: {ClassId?.ToString() ?? "NULL"},");
            sb.AppendLine($"name: {Name ?? "NULL"},");
            sb.AppendLine($"project_name: {ProjectName ?? "NULL"},");
            sb.AppendLine($"tags: {Tags ?? "NULL"},");
            sb.AppendLine($"created_by: {CreatedBy ?? "NULL"},");
            sb.AppendLine($"created_at: {CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss.fffffff") ?? "NULL"},");
            sb.AppendLine($"modified_by: {ModifiedBy ?? "NULL"},");
            sb.AppendLine($"modified_at: {ModifiedAt?.ToString("yyyy-MM-dd HH:mm:ss.fffffff") ?? "NULL"},");
            sb.AppendLine($"archived_at: {ArchivedAt?.ToString("yyyy-MM-dd HH:mm:ss.fffffff") ?? "NULL"}");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}