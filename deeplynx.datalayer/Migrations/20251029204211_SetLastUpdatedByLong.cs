using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class SetLastUpdatedByLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.data_sources 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_data_sources_last_updated_by",
                schema: "deeplynx",
                table: "data_sources",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.edges 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_edges_last_updated_by",
                schema: "deeplynx",
                table: "edges",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.events 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_events_last_updated_by",
                schema: "deeplynx",
                table: "events",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.groups 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_groups_last_updated_by",
                schema: "deeplynx",
                table: "groups",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.historical_edges 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_last_updated_by",
                schema: "deeplynx",
                table: "historical_edges",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.object_storages 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_object_storages_last_updated_by",
                schema: "deeplynx",
                table: "object_storages",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.organizations 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_organizations_last_updated_by",
                schema: "deeplynx",
                table: "organizations",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.permissions 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_permissions_last_updated_by",
                schema: "deeplynx",
                table: "permissions",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.projects 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_projects_last_updated_by",
                schema: "deeplynx",
                table: "projects",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.records 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_records_last_updated_by",
                schema: "deeplynx",
                table: "records",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.relationships 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_relationships_last_updated_by",
                schema: "deeplynx",
                table: "relationships",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.roles 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_roles_last_updated_by",
                schema: "deeplynx",
                table: "roles",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.sensitivity_labels 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_sensitivity_labels_last_updated_by",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.subscriptions 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_last_updated_by",
                schema: "deeplynx",
                table: "subscriptions",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.tags 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");
            migrationBuilder.CreateIndex(
                name: "idx_tags_last_updated_by",
                schema: "deeplynx",
                table: "tags",
                column: "last_updated_by");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.actions 
                ALTER COLUMN last_updated_by TYPE bigint USING NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "idx_actions_last_updated_by",
                schema: "deeplynx",
                table: "actions",
                column: "last_updated_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.DropIndex(
                name: "idx_data_sources_last_updated_by",
                schema: "deeplynx",
                table: "data_sources");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.data_sources 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_edges_last_updated_by",
                schema: "deeplynx",
                table: "edges");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.edges 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_events_last_updated_by",
                schema: "deeplynx",
                table: "events");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.events 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_groups_last_updated_by",
                schema: "deeplynx",
                table: "groups");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.groups 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_historical_edges_last_updated_by",
                schema: "deeplynx",
                table: "historical_edges");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.historical_edges 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_object_storages_last_updated_by",
                schema: "deeplynx",
                table: "object_storages");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.object_storages 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_organizations_last_updated_by",
                schema: "deeplynx",
                table: "organizations");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.organizations 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_permissions_last_updated_by",
                schema: "deeplynx",
                table: "permissions");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.permissions 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_projects_last_updated_by",
                schema: "deeplynx",
                table: "projects");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.projects 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_records_last_updated_by",
                schema: "deeplynx",
                table: "records");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.records 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_relationships_last_updated_by",
                schema: "deeplynx",
                table: "relationships");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.relationships 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_roles_last_updated_by",
                schema: "deeplynx",
                table: "roles");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.roles 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_sensitivity_labels_last_updated_by",
                schema: "deeplynx",
                table: "sensitivity_labels");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.sensitivity_labels 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_subscriptions_last_updated_by",
                schema: "deeplynx",
                table: "subscriptions");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.subscriptions 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_tags_last_updated_by",
                schema: "deeplynx",
                table: "tags");
            
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.tags 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
            
            migrationBuilder.DropIndex(
                name: "idx_actions_last_updated_by",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.actions 
                ALTER COLUMN last_updated_by TYPE text 
                USING last_updated_by::text;
            ");
        }
    }
}
