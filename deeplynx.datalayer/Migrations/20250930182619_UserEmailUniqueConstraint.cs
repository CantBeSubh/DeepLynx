using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UserEmailUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_users_email",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                schema: "deeplynx",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_users_email",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                schema: "deeplynx",
                table: "users",
                column: "email");
        }
    }
}
