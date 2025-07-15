using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CreateArchiveCascadeForDataSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // archive data source and attached edge and record mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_data_source(arc_data_source_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.data_sources SET archived_at = arc_time WHERE id = arc_data_source_id;
                    UPDATE deeplynx.record_mappings SET archived_at = arc_time WHERE data_source_id = arc_data_source_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE data_source_id = arc_data_source_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_data_source;");
        }
    }
}
