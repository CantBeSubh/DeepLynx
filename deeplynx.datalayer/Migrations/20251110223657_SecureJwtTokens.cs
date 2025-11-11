using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class SecureJwtTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_used_at",
                schema: "deeplynx",
                table: "oauth_tokens");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                schema: "deeplynx",
                table: "oauth_tokens",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "application_id",
                schema: "deeplynx",
                table: "api_keys",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_api_keys_application_id",
                schema: "deeplynx",
                table: "api_keys",
                column: "application_id");

            migrationBuilder.AddForeignKey(
                name: "api_keys_application_id_fkey",
                schema: "deeplynx",
                table: "api_keys",
                column: "application_id",
                principalSchema: "deeplynx",
                principalTable: "oauth_applications",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "api_keys_application_id_fkey",
                schema: "deeplynx",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "idx_api_keys_application_id",
                schema: "deeplynx",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "application_id",
                schema: "deeplynx",
                table: "api_keys");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                schema: "deeplynx",
                table: "oauth_tokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_used_at",
                schema: "deeplynx",
                table: "oauth_tokens",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
