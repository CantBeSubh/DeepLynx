using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "actions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<string>(type: "jsonb", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("actions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "actions_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    operation = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    properties = table.Column<string>(type: "jsonb", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("events_pkey", x => x.id);
                    table.ForeignKey(
                        name: "events_dataSource_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "events_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "events_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    action_id = table.Column<long>(type: "bigint", nullable: false),
                    operation = table.Column<string>(type: "text", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("subscriptions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "subscriptions_action_id_fkey",
                        column: x => x.action_id,
                        principalSchema: "deeplynx",
                        principalTable: "actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "subscriptions_dataSource_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "subscriptions_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "subscriptions_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_actions_id",
                schema: "deeplynx",
                table: "actions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_project_id",
                schema: "deeplynx",
                table: "actions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_id",
                schema: "deeplynx",
                table: "events",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_events_project_id",
                schema: "deeplynx",
                table: "events",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_user_id",
                schema: "deeplynx",
                table: "events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_data_source_id",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_entity_type",
                schema: "deeplynx",
                table: "subscriptions",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_project_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_user_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_action_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "action_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_data_source_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "data_source_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "actions",
                schema: "deeplynx");
        }
    }
}