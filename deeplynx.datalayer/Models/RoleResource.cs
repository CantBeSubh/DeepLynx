using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("role_resources", Schema = "deeplynx")]
[Index("DataSourceId", Name = "idx_role_resources_data_source_id")]
[Index("RecordId", Name = "idx_role_resources_record_id")]
[Index("RoleId", Name = "idx_role_resources_role_id")]
[Index("TagId", Name = "idx_role_resources_tag_id")]
public partial class RoleResource
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("role_id")]
    public long RoleId { get; set; }

    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("tag_id")]
    public long? TagId { get; set; }

    [Column("record_id")]
    public long? RecordId { get; set; }

    [Column("has_access")]
    public bool? HasAccess { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("RoleResources")]
    public virtual DataSource? DataSource { get; set; }

    [ForeignKey("RecordId")]
    [InverseProperty("RoleResources")]
    public virtual Record? Record { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("RoleResources")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("RoleResources")]
    public virtual Tag? Tag { get; set; }
}
