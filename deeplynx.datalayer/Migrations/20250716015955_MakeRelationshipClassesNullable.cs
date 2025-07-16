using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class MakeRelationshipClassesNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "relationships_destination_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropForeignKey(
                name: "relationships_origin_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.AlterColumn<long>(
                name: "origin_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "destination_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "relationships_destination_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "destination_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "relationships_origin_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "origin_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "relationships_destination_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropForeignKey(
                name: "relationships_origin_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.AlterColumn<long>(
                name: "origin_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "destination_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "relationships_destination_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "destination_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "relationships_origin_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "origin_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
