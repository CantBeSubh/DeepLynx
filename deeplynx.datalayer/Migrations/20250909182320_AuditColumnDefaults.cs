using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AuditColumnDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "sensitivity_labels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "actions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "groups");

            migrationBuilder.AlterColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "actions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
