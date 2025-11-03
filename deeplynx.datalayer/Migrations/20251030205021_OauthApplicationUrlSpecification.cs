using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class OauthApplicationUrlSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "redirect_uris",
                table: "oauth_applications");

            migrationBuilder.AddColumn<string>(
                name: "base_url",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "callback_url",
                table: "oauth_applications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "base_url",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "callback_url",
                table: "oauth_applications");

            migrationBuilder.AddColumn<List<string>>(
                name: "redirect_uris",
                table: "oauth_applications",
                type: "jsonb",
                nullable: false);
        }
    }
}
