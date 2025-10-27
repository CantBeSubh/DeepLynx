using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class SetClassLastUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.classes 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "idx_classes_last_updated_by",
                schema: "deeplynx",
                table: "classes",
                column: "last_updated_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "idx_classes_last_updated_by",
                schema: "deeplynx",
                table: "classes");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.classes 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
        }
    }
}
