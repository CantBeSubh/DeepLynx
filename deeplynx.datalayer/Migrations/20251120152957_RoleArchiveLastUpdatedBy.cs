using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RoleArchiveLastUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_role(
                    arc_role_id INTEGER, 
                    arc_time TIMESTAMP WITHOUT TIME ZONE,
                    arc_by INTEGER
                    )
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.roles 
                        SET 
                            is_archived = TRUE,
                            last_updated_at = arc_time,
                            last_updated_by = arc_by
                        WHERE id = arc_role_id;
                    -- remove this role from anyone who holds it
                    UPDATE deeplynx.project_members 
                        SET role_id = NULL
                        WHERE role_id = arc_role_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_role(arc_role_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.roles 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE id = arc_role_id;
                    -- remove this role from anyone who holds it
                    UPDATE deeplynx.project_members 
                        SET role_id = NULL
                        WHERE role_id = arc_role_id;
                END;
                $$;
            ");
        }
    }
}
