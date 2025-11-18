using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class EventSubscriptionActionScopeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "actions_project_id_fkey",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropForeignKey(
                name: "subscriptions_action_id_fkey",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "subscriptions_dataSource_id_fkey",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "subscriptions_project_id_fkey",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "subscriptions_user_id_fkey",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "idx_unique_subscription",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.RenameIndex(
                name: "idx_project_id",
                schema: "deeplynx",
                table: "actions",
                newName: "idx_actions_project_id");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "actions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "actions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_organization_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_unique_subscription_with_project",
                schema: "deeplynx",
                table: "subscriptions",
                columns: new[] { "user_id", "action_id", "operation", "organization_id", "project_id", "data_source_id", "entity_type", "entity_id" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_unique_subscription_without_project",
                schema: "deeplynx",
                table: "subscriptions",
                columns: new[] { "user_id", "action_id", "operation", "organization_id", "data_source_id", "entity_type", "entity_id" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Events_ProjectId_OrganizationId_XOR",
                schema: "deeplynx",
                table: "events",
                sql: "(project_id IS NOT NULL AND organization_id IS NULL) OR (project_id IS NULL AND organization_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "idx_actions_organization_id",
                schema: "deeplynx",
                table: "actions",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "FK_actions_organizations_organization_id",
                schema: "deeplynx",
                table: "actions",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_actions_projects_project_id",
                schema: "deeplynx",
                table: "actions",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_organizations_organization_id",
                schema: "deeplynx",
                table: "events",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_actions_action_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "action_id",
                principalSchema: "deeplynx",
                principalTable: "actions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_data_sources_data_source_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_organizations_organization_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_projects_project_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_users_user_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "user_id",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_actions_organizations_organization_id",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropForeignKey(
                name: "FK_actions_projects_project_id",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropForeignKey(
                name: "FK_events_organizations_organization_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_actions_action_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_data_sources_data_source_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_organizations_organization_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_projects_project_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_users_user_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "idx_subscriptions_organization_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "idx_unique_subscription_with_project",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "idx_unique_subscription_without_project",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Events_ProjectId_OrganizationId_XOR",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropIndex(
                name: "idx_actions_organization_id",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.RenameIndex(
                name: "idx_actions_project_id",
                schema: "deeplynx",
                table: "actions",
                newName: "idx_project_id");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "actions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_unique_subscription",
                schema: "deeplynx",
                table: "subscriptions",
                columns: new[] { "user_id", "action_id", "operation", "project_id", "data_source_id", "entity_type", "entity_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "actions_project_id_fkey",
                schema: "deeplynx",
                table: "actions",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "subscriptions_action_id_fkey",
                schema: "deeplynx",
                table: "subscriptions",
                column: "action_id",
                principalSchema: "deeplynx",
                principalTable: "actions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "subscriptions_dataSource_id_fkey",
                schema: "deeplynx",
                table: "subscriptions",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "subscriptions_project_id_fkey",
                schema: "deeplynx",
                table: "subscriptions",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "subscriptions_user_id_fkey",
                schema: "deeplynx",
                table: "subscriptions",
                column: "user_id",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
