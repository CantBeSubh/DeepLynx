using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CascadingConstraintsForFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: needing to do raw SQL here because entity framework doesn't have an if exists/if not exists feature for postgres
            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.events 
                DROP CONSTRAINT IF EXISTS ""FK_events_data_sources_data_source_id"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.events
                DROP CONSTRAINT IF EXISTS ""FK_events_projects_project_id"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.object_storages 
                DROP CONSTRAINT IF EXISTS ""FK_object_storages_organizations_organization_id"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.object_storages 
                DROP CONSTRAINT IF EXISTS ""object_storage_project_id_fkey"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.project_members 
                DROP CONSTRAINT IF EXISTS ""project_members_role_id_fkey"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE deeplynx.saved_searches 
                DROP CONSTRAINT IF EXISTS ""saved_searches_user_id_fkey"";
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'saved_searches' 
                        AND constraint_name = 'saved_searches_user_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.saved_searches 
                        ADD CONSTRAINT saved_searches_user_id_fkey 
                        FOREIGN KEY (user_id) 
                        REFERENCES deeplynx.users(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'project_members' 
                        AND constraint_name = 'project_members_role_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.project_members 
                        ADD CONSTRAINT project_members_role_id_fkey 
                        FOREIGN KEY (role_id) 
                        REFERENCES deeplynx.roles(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'events' 
                        AND constraint_name = 'events_data_source_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.events 
                        ADD CONSTRAINT events_data_source_id_fkey 
                        FOREIGN KEY (data_source_id) 
                        REFERENCES deeplynx.data_sources(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'events' 
                        AND constraint_name = 'events_organization_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.events 
                        ADD CONSTRAINT events_organization_id_fkey 
                        FOREIGN KEY (organization_id) 
                        REFERENCES deeplynx.organizations(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'events' 
                        AND constraint_name = 'events_project_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.events 
                        ADD CONSTRAINT events_project_id_fkey 
                        FOREIGN KEY (project_id) 
                        REFERENCES deeplynx.projects(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'object_storages' 
                        AND constraint_name = 'object_storage_organization_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.object_storages 
                        ADD CONSTRAINT object_storage_organization_id_fkey 
                        FOREIGN KEY (organization_id) 
                        REFERENCES deeplynx.organizations(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_schema = 'deeplynx' 
                        AND table_name = 'object_storages' 
                        AND constraint_name = 'object_storage_project_id_fkey'
                    ) THEN
                        ALTER TABLE deeplynx.object_storages 
                        ADD CONSTRAINT object_storage_project_id_fkey 
                        FOREIGN KEY (project_id) 
                        REFERENCES deeplynx.projects(id) 
                        ON DELETE CASCADE;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_data_source_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "events_organization_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "object_storage_organization_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "saved_searches_user_id_fkey",
                schema: "deeplynx",
                table: "saved_searches");

            migrationBuilder.AddForeignKey(
                name: "saved_searches_user_id_fkey",
                schema: "deeplynx",
                table: "saved_searches",
                column: "user_id",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_data_sources_data_source_id",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_projects_project_id",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_object_storages_organizations_organization_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");
        }
    }
}
