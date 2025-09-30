using System.ComponentModel.DataAnnotations.Schema;

public class EventResponseDto
{
    [Column("id")]
    public long Id { get; set; }

    [Column("operation")]
    public string Operation { get; set; }

    [Column("entity_type")]
    public string EntityType { get; set; }

    [Column("entity_id")]
    public long? EntityId { get; set; }

    [Column("project_id")]
    public long? ProjectId { get; set; }

    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("properties")]
    public string Properties { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    
}

// using System;
// using System.ComponentModel.DataAnnotations.Schema;
// using System.Text.Json.Serialization;
//
// public class EventResponseDto
// {
//     [Column("id")]
//     [JsonPropertyName("id")]
//     public long Id { get; set; }
//
//     [Column("operation")]
//     [JsonPropertyName("operation")]
//     public string Operation { get; set; }
//
//     [Column("entity_type")]
//     [JsonPropertyName("entityType")]
//     public string EntityType { get; set; }
//
//     [Column("entity_id")]
//     [JsonPropertyName("entityId")]
//     public long? EntityId { get; set; }
//
//     [Column("project_id")]
//     [JsonPropertyName("projectId")]
//     public long? ProjectId { get; set; }
//
//     [Column("organization_id")]
//     [JsonPropertyName("organizationId")]
//     public long? OrganizationId { get; set; }
//
//     [Column("data_source_id")]
//     [JsonPropertyName("dataSourceId")]
//     public long? DataSourceId { get; set; }
//
//     [Column("properties")]
//     [JsonPropertyName("properties")]
//     public string Properties { get; set; }
//
//     [JsonPropertyName("lastUpdatedAt")]
//     public DateTime LastUpdatedAt { get; set; }
//
//     [JsonPropertyName("lastUpdatedBy")]
//     public string? LastUpdatedBy { get; set; }
// }
