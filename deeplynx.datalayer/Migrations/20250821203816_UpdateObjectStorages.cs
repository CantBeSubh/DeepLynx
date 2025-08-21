using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateObjectStorages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "object_storage_name",
                schema: "deeplynx",
                table: "historical_records",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "object_storages",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<string>(type: "jsonb", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    @default = table.Column<bool>(name: "default", type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("object_storage_pkey", x => x.id);
                    table.ForeignKey(
                        name: "object_storage_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_records_object_storage_id",
                schema: "deeplynx",
                table: "records",
                column: "object_storage_id");

            migrationBuilder.CreateIndex(
                name: "idx_object_storage_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_object_storages_project_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "records_object_storage_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "object_storage_id",
                principalSchema: "deeplynx",
                principalTable: "object_storages",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "records_object_storage_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropTable(
                name: "object_storages",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "IX_records_object_storage_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "object_storage_name",
                schema: "deeplynx",
                table: "historical_records");
        }
    }
}
