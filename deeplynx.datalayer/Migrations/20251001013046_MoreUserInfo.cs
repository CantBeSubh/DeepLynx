using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class MoreUserInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_project",
                schema: "deeplynx");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "deeplynx",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "sso_id",
                schema: "deeplynx",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                schema: "deeplynx",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_sso_id",
                schema: "deeplynx",
                table: "users",
                column: "sso_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_users_sso_id",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropColumn(
                name: "sso_id",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropColumn(
                name: "username",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.CreateTable(
                name: "user_project",
                schema: "deeplynx",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_project_pkey", x => new { x.user_id, x.project_id });
                    table.ForeignKey(
                        name: "user_project_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_project_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_user_project_project_id",
                schema: "deeplynx",
                table: "user_project",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_project_user_id",
                schema: "deeplynx",
                table: "user_project",
                column: "user_id");
        }
    }
}
