using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("users", Schema = "deeplynx")]
[Index("Id", Name = "idx_users_id")]
public partial class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("type")]
    public string Type { get; set; } = null!;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("modified_at", TypeName = "timestamp without time zone")]
    public DateTime? ModifiedAt { get; set; }

    [Column("deleted_at", TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
