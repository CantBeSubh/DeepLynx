using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace deeplynx.datalayer.Models;

[Table("organizations", Schema = "deeplynx")]
[Index("Id", Name = "idx_organizations_id")]
public partial class Organization
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")] 
    public string Name { get; set; } = null!;
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }
    
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [InverseProperty("Organization")]
    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();
    
    [InverseProperty("Organization")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    
    [InverseProperty("Organization")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

}