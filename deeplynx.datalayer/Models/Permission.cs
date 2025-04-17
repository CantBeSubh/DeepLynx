using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("permissions", Schema = "deeplynx")]
public partial class Permission
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("role_id")]
    public long RoleId { get; set; }

    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("record_id")]
    public long? RecordId { get; set; }

    [Column("access_type")]
    public string? AccessType { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("modified_at", TypeName = "timestamp without time zone")]
    public DateTime ModifiedAt { get; set; }

    [Column("deleted_at", TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("Permissions")]
    public virtual DataSource? DataSource { get; set; }

    [ForeignKey("RecordId")]
    [InverseProperty("Permissions")]
    public virtual Record? Record { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Permissions")]
    public virtual Role Role { get; set; } = null!;
}
