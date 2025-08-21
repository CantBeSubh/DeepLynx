using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixEventCascadeBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_user_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropIndex(
                name: "idx_events_user_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "modified_at",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
            
            migrationBuilder.DropForeignKey(
                name: "events_dataSource_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.AddForeignKey(
                name: "events_dataSource_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                schema: "deeplynx",
                table: "events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "user_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_events_user_id",
                schema: "deeplynx",
                table: "events",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "events_user_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "user_id",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
            
            migrationBuilder.DropForeignKey(
                name: "events_dataSource_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.AddForeignKey(
                name: "events_dataSource_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
