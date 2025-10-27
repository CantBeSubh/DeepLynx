using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class SetDataSourceEdgeEventGroupHistoricalEdgeLastUpdatedBy : Migration
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
        }
    }
}
