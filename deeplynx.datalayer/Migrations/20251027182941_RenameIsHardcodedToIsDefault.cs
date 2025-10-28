using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsHardcodedToIsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
            name: "is_hardcoded",
            schema: "deeplynx",
            table: "permissions",
            newName: "is_default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
            name: "is_default",
            schema: "deeplynx",
            table: "permissions",
            newName: "is_hardcoded");
        }
    }
}
