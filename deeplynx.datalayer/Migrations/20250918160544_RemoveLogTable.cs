using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logs",
                schema: "deeplynx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "logs",
                schema: "deeplynx",
                columns: table => new
                {
                    exception = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: true),
                    log_event = table.Column<string>(type: "jsonb", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    message_template = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                });
        }
    }
}
