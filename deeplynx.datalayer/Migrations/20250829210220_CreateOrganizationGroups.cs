using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CreateOrganizationGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_sysadmin",
                schema: "deeplynx",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    organization_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("groups_pkey", x => x.id);
                    table.ForeignKey(
                        name: "groups_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_users",
                schema: "deeplynx",
                columns: table => new
                {
                    organization_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    is_org_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_user_pkey", x => new { x.organization_id, x.user_id });
                    table.ForeignKey(
                        name: "organization_users_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "organization_users_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_groups_id",
                schema: "deeplynx",
                table: "groups",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_groups_organization_id",
                schema: "deeplynx",
                table: "groups",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_organization_users_organization_id",
                schema: "deeplynx",
                table: "organization_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_organization_users_user_id",
                schema: "deeplynx",
                table: "organization_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "unique_organization_user_ids",
                schema: "deeplynx",
                table: "organization_users",
                columns: new[] { "organization_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_organizations_id",
                schema: "deeplynx",
                table: "organizations",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "groups",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "organization_users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "deeplynx");

            migrationBuilder.DropColumn(
                name: "is_sysadmin",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
