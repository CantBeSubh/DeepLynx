using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[PrimaryKey("RecordMappingId", "TagId")]
[Table("record_mapping_tags", Schema = "deeplynx")]
[Index("RecordMappingId", Name = "idx_record_mapping_tags_record_mapping_id")]
[Index("TagId", Name = "idx_record_mapping_tags_tag_id")]
public partial class RecordMappingTag
{
    [Key]
    public long RecordMappingId { get; set; }

    [Key]
    [Column("tag_id")]
    public long TagId { get; set; }

    [Column("record_mapping_id")]
    public long RecordMappingId1 { get; set; }

    [ForeignKey("RecordMappingId")]
    [InverseProperty("RecordMappingTags")]
    public virtual RecordMapping RecordMapping { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("RecordMappingTags")]
    public virtual Tag Tag { get; set; } = null!;
}
