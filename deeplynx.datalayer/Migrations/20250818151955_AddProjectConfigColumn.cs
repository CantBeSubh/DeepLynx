using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectConfigColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "config",
                schema: "deeplynx",
                table: "projects",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{\"edgeRecordsMutable\":false,\"ontologyMutable\":false,\"tagsMutable\":false}'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "config",
                schema: "deeplynx",
                table: "projects");
        }
    }
}
