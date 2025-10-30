using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddOauthTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "oauth_applications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret_hash = table.Column<string>(type: "text", nullable: false),
                    redirect_uris = table.Column<List<string>>(type: "jsonb", nullable: false),
                    app_owner_email = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("oauth_applications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_oauth_applications_users_last_updated_by",
                        column: x => x.last_updated_by,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "oauth_tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_used_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("oauth_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "oauth_tokens_application_id_fkey",
                        column: x => x.application_id,
                        principalTable: "oauth_applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "oauth_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_oauth_applications_client_id",
                table: "oauth_applications",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "idx_oauth_applications_id",
                table: "oauth_applications",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_oauth_applications_last_updated_by",
                table: "oauth_applications",
                column: "last_updated_by");

            migrationBuilder.CreateIndex(
                name: "idx_oauth_tokens_application_id",
                table: "oauth_tokens",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "idx_oauth_tokens_id",
                table: "oauth_tokens",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_oauth_tokens_user_id",
                table: "oauth_tokens",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "oauth_tokens");

            migrationBuilder.DropTable(
                name: "oauth_applications");
        }
    }
}
