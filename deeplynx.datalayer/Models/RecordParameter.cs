using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("record_parameters", Schema = "deeplynx")]
public partial class RecordParameter
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("record_params", TypeName = "jsonb")]
    public string RecordParams { get; set; } = null!;

    [Column("class_id")]
    public long ClassId { get; set; }

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

    [ForeignKey("ClassId")]
    [InverseProperty("RecordParameters")]
    public virtual Class Class { get; set; } = null!;
}
