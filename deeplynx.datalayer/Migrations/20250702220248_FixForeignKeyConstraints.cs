using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                schema: "deeplynx");

            migrationBuilder.DropForeignKey(
                name: "records_mapping_id_fkey",
                table: "records",
                schema: "deeplynx");

            migrationBuilder.DropForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                schema: "deeplynx");

            migrationBuilder.DropForeignKey(
                name: "edges_mapping_id_fkey",
                table: "edges",
                schema: "deeplynx");

            migrationBuilder.AlterColumn<int>(
                name: "mapping_id",
                table: "records",
                schema: "deeplynx",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "class_id",
                table: "records",
                schema: "deeplynx",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "mapping_id",
                table: "edges",
                schema: "deeplynx",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "relationship_id",
                table: "edges",
                schema: "deeplynx",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                schema: "deeplynx",
                column: "class_id",
                principalTable: "classes",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "records_mapping_id_fkey",
                table: "records",
                schema: "deeplynx",
                column: "mapping_id",
                principalTable: "record_mappings",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                schema: "deeplynx",
                column: "relationship_id",
                principalTable: "relationships",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edges_mapping_id_fkey",
                table: "edges",
                schema: "deeplynx",
                column: "mapping_id",
                principalTable: "edge_mappings",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                schema: "deeplynx");

            migrationBuilder.DropForeignKey(
                name: "records_mapping_id_fkey",
                table: "records",
                schema: "deeplynx");

            migrationBuilder.DropForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                schema: "deeplynx");

            migrationBuilder.DropForeignKey(
                name: "edges_mapping_id_fkey",
                table: "edges",
                schema: "deeplynx");

            migrationBuilder.AlterColumn<int>(
                name: "mapping_id",
                table: "records",
                schema: "deeplynx",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "class_id",
                table: "records",
                schema: "deeplynx",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mapping_id",
                table: "edges",
                schema: "deeplynx",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "relationship_id",
                table: "edges",
                schema: "deeplynx",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                schema: "deeplynx",
                column: "class_id",
                principalTable: "classes",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "records_mapping_id_fkey",
                table: "records",
                schema: "deeplynx",
                column: "mapping_id",
                principalTable: "record_mappings",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                schema: "deeplynx",
                column: "relationship_id",
                principalTable: "relationships",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edges_mapping_id_fkey",
                table: "edges",
                schema: "deeplynx",
                column: "mapping_id",
                principalTable: "edge_mappings",
                principalSchema: "deeplynx",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
