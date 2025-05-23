using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddEdgeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "edges_pkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "edges_data_source_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.AlterColumn<long>(
                name: "data_source_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "idx_edges_id",
                schema: "deeplynx",
                table: "edges",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "edges_pkey",
                schema: "deeplynx",
                table: "edges",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "edges_data_source_id_fkey",
                schema: "deeplynx",
                table: "edges",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edges_data_source_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropPrimaryKey(
                name: "edges_pkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.AlterColumn<long>(
                name: "data_source_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "edges_pk",
                schema: "deeplynx",
                table: "edges",
                columns: new[] { "origin_id", "destination_id" });

            migrationBuilder.AddForeignKey(
                name: "edges_data_source_id_fkey",
                schema: "deeplynx",
                table: "edges",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id");
        }
    }
}
