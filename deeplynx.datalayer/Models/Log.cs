using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Keyless]
[Table("logs", Schema = "deeplynx")]
public partial class Log
{
    [Column("message")]
    public string? Message { get; set; }

    [Column("message_template")]
    public string? MessageTemplate { get; set; }

    [Column("level")]
    public int? Level { get; set; }

    [Column("timestamp", TypeName = "timestamp without time zone")]
    public DateTime? Timestamp { get; set; }

    [Column("exception")]
    public string? Exception { get; set; }

    [Column("log_event", TypeName = "jsonb")]
    public string? LogEvent { get; set; }
}
