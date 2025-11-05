using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixOauthSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "oauth_tokens",
                newName: "oauth_tokens",
                newSchema: "deeplynx");

            migrationBuilder.RenameTable(
                name: "oauth_applications",
                newName: "oauth_applications",
                newSchema: "deeplynx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "oauth_tokens",
                schema: "deeplynx",
                newName: "oauth_tokens");

            migrationBuilder.RenameTable(
                name: "oauth_applications",
                schema: "deeplynx",
                newName: "oauth_applications");
        }
    }
}
