using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class OrgRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "permissions_unique_org_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_org_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_project_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_project_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_object_storages_ProjectXorOrg",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.RenameIndex(
                name: "IX_subscriptions_data_source_id",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "idx_subscriptions_data_source_id");

            migrationBuilder.RenameIndex(
                name: "IX_subscriptions_action_id",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "idx_subscriptions_action_id");

            migrationBuilder.RenameIndex(
                name: "IX_records_object_storage_id",
                schema: "deeplynx",
                table: "records",
                newName: "idx_records_object_storage_id");

            migrationBuilder.RenameIndex(
                name: "IX_object_storages_project_id",
                schema: "deeplynx",
                table: "object_storages",
                newName: "idx_object_storages_project_id");

            migrationBuilder.RenameIndex(
                name: "IX_object_storages_organization_id",
                schema: "deeplynx",
                table: "object_storages",
                newName: "idx_object_storages_organization_id");

            migrationBuilder.RenameIndex(
                name: "idx_object_storage_id",
                schema: "deeplynx",
                table: "object_storages",
                newName: "idx_object_storages_id");

            migrationBuilder.RenameIndex(
                name: "idx_project_id",
                schema: "deeplynx",
                table: "actions",
                newName: "idx_actions_project_id");

            // ========== ADD NEW COLUMNS AS NULLABLE ==========
            
            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "tags",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "data_sources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "classes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "actions",
                type: "bigint",
                nullable: true);

            // ========== BACKFILL DATA ==========
            
            // Backfill tags
            migrationBuilder.Sql(@"
                UPDATE deeplynx.tags t
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE t.project_id = p.id
                  AND t.organization_id IS NULL;
            ");

            // Backfill subscriptions
            migrationBuilder.Sql(@"
                UPDATE deeplynx.subscriptions s
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE s.project_id = p.id
                  AND s.organization_id IS NULL;
            ");

            // Backfill sensitivity_labels (existing nullable column)
            migrationBuilder.Sql(@"
                UPDATE deeplynx.sensitivity_labels sl
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE sl.project_id = p.id
                  AND sl.organization_id IS NULL;
            ");

            // Backfill relationships
            migrationBuilder.Sql(@"
                UPDATE deeplynx.relationships r
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE r.project_id = p.id
                  AND r.organization_id IS NULL;
            ");

            // Backfill records
            migrationBuilder.Sql(@"
                UPDATE deeplynx.records r
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE r.project_id = p.id
                  AND r.organization_id IS NULL;
            ");

            // Permissions r weird so not doing this
            // Backfill permissions (existing nullable column)
            // migrationBuilder.Sql(@"
            //     UPDATE deeplynx.permissions perm
            //     SET organization_id = p.organization_id
            //     FROM deeplynx.projects p
            //     WHERE perm.project_id = p.id
            //       AND perm.organization_id IS NULL;
            // ");

            // Backfill object_storages (existing nullable column)
            migrationBuilder.Sql(@"
                UPDATE deeplynx.object_storages os
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE os.project_id = p.id
                  AND os.organization_id IS NULL;
            ");

            // Backfill historical_records
            migrationBuilder.Sql(@"
                UPDATE deeplynx.historical_records hr
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE hr.project_id = p.id
                  AND hr.organization_id IS NULL;
            ");

            // Backfill historical_edges
            migrationBuilder.Sql(@"
                UPDATE deeplynx.historical_edges he
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE he.project_id = p.id
                  AND he.organization_id IS NULL;
            ");

            // Backfill edges
            migrationBuilder.Sql(@"
                UPDATE deeplynx.edges e
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE e.project_id = p.id
                  AND e.organization_id IS NULL;
            ");

            // Backfill data_sources
            migrationBuilder.Sql(@"
                UPDATE deeplynx.data_sources ds
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE ds.project_id = p.id
                  AND ds.organization_id IS NULL;
            ");

            // Backfill classes
            migrationBuilder.Sql(@"
                UPDATE deeplynx.classes c
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE c.project_id = p.id
                  AND c.organization_id IS NULL;
            ");

            // Backfill actions
            migrationBuilder.Sql(@"
                UPDATE deeplynx.actions a
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE a.project_id = p.id
                  AND a.organization_id IS NULL;
            ");

            // ========== ALTER COLUMNS TO NOT NULL ==========
            
            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "tags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "records",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // Permissions r weird so not doing this
            // migrationBuilder.AlterColumn<long>(
            //     name: "organization_id",
            //     schema: "deeplynx",
            //     table: "permissions",
            //     type: "bigint",
            //     nullable: false,
            //     oldClrType: typeof(long),
            //     oldType: "bigint",
            //     oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "object_storages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "data_sources",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "classes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "actions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // ========== CREATE INDEXES ==========

            migrationBuilder.CreateIndex(
                name: "idx_tags_organization_id",
                schema: "deeplynx",
                table: "tags",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_organization_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "unique_organization_sensitivity_label_name",
                schema: "deeplynx",
                table: "sensitivity_labels",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "unique_project_sensitivity_label_name",
                schema: "deeplynx",
                table: "sensitivity_labels",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_organization_id",
                schema: "deeplynx",
                table: "relationships",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_organization_id",
                schema: "deeplynx",
                table: "records",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "label_id", "action" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "resource", "action" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_project_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "project_id", "label_id", "action" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_project_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "project_id", "resource", "action" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "unique_organization_object_storage_name",
                schema: "deeplynx",
                table: "object_storages",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "unique_project_object_storage_name",
                schema: "deeplynx",
                table: "object_storages",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_organization_id",
                schema: "deeplynx",
                table: "historical_records",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_project_id",
                schema: "deeplynx",
                table: "historical_records",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_organization_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_project_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_organization_id",
                schema: "deeplynx",
                table: "edges",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_data_sources_organization_id",
                schema: "deeplynx",
                table: "data_sources",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_classes_organization_id",
                schema: "deeplynx",
                table: "classes",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_actions_organization_id",
                schema: "deeplynx",
                table: "actions",
                column: "organization_id");

            // ========== ADD FOREIGN KEYS ==========

            migrationBuilder.AddForeignKey(
                name: "actions_organization_id_fkey",
                schema: "deeplynx",
                table: "actions",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "classes_organization_id_fkey",
                schema: "deeplynx",
                table: "classes",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "data_sources_organization_id_fkey",
                schema: "deeplynx",
                table: "data_sources",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edges_organization_id_fkey",
                schema: "deeplynx",
                table: "edges",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "historical_edges_organization_id_fkey",
                schema: "deeplynx",
                table: "historical_edges",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "historical_edges_project_id_fkey",
                schema: "deeplynx",
                table: "historical_edges",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "historical_records_organization_id_fkey",
                schema: "deeplynx",
                table: "historical_records",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "historical_records_project_id_fkey",
                schema: "deeplynx",
                table: "historical_records",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "records_organization_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "relationships_organization_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "subscriptions_organization_id_fkey",
                schema: "deeplynx",
                table: "subscriptions",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "actions_organization_id_fkey",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropForeignKey(
                name: "classes_organization_id_fkey",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropForeignKey(
                name: "data_sources_organization_id_fkey",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropForeignKey(
                name: "edges_organization_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "historical_edges_organization_id_fkey",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropForeignKey(
                name: "historical_edges_project_id_fkey",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropForeignKey(
                name: "historical_records_organization_id_fkey",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropForeignKey(
                name: "historical_records_project_id_fkey",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropForeignKey(
                name: "records_organization_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropForeignKey(
                name: "relationships_organization_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropForeignKey(
                name: "subscriptions_organization_id_fkey",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "idx_tags_organization_id",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "idx_subscriptions_organization_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "unique_organization_sensitivity_label_name",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropIndex(
                name: "unique_project_sensitivity_label_name",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropIndex(
                name: "idx_relationships_organization_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_records_organization_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "permissions_unique_org_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_org_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_project_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_project_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "unique_organization_object_storage_name",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropIndex(
                name: "unique_project_object_storage_name",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropIndex(
                name: "idx_historical_records_organization_id",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropIndex(
                name: "idx_historical_records_project_id",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropIndex(
                name: "idx_historical_edges_organization_id",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropIndex(
                name: "idx_historical_edges_project_id",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_organization_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_data_sources_organization_id",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "idx_classes_organization_id",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "idx_actions_organization_id",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.RenameIndex(
                name: "idx_subscriptions_data_source_id",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "IX_subscriptions_data_source_id");

            migrationBuilder.RenameIndex(
                name: "idx_subscriptions_action_id",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "IX_subscriptions_action_id");

            migrationBuilder.RenameIndex(
                name: "idx_records_object_storage_id",
                schema: "deeplynx",
                table: "records",
                newName: "IX_records_object_storage_id");

            migrationBuilder.RenameIndex(
                name: "idx_object_storages_project_id",
                schema: "deeplynx",
                table: "object_storages",
                newName: "IX_object_storages_project_id");

            migrationBuilder.RenameIndex(
                name: "idx_object_storages_organization_id",
                schema: "deeplynx",
                table: "object_storages",
                newName: "IX_object_storages_organization_id");

            migrationBuilder.RenameIndex(
                name: "idx_object_storages_id",
                schema: "deeplynx",
                table: "object_storages",
                newName: "idx_object_storage_id");

            migrationBuilder.RenameIndex(
                name: "idx_actions_project_id",
                schema: "deeplynx",
                table: "actions",
                newName: "idx_project_id");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "object_storages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "label_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "resource", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_project_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "project_id", "label_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_project_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "project_id", "resource", "action" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_object_storages_ProjectXorOrg",
                schema: "deeplynx",
                table: "object_storages",
                sql: "(project_id IS NOT NULL AND organization_id IS NULL) OR (project_id IS NULL AND organization_id IS NOT NULL)");
        }
    }
}
