using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddEdgeAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "edges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                schema: "deeplynx",
                table: "edges",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "modified_at",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "edges");
        }
    }
}
