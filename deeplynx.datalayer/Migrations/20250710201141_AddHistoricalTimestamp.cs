using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricalTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "record_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                column: "last_updated_at");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                column: "last_updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_historical_records_last_updated_at",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropIndex(
                name: "idx_historical_edges_last_updated_at",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.AlterColumn<long>(
                name: "record_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
