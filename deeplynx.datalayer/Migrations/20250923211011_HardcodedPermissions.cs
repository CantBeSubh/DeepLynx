using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class HardcodedPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO deeplynx.permissions (name, description, action, resource, is_hardcoded) VALUES
                    ('Write Projects', 'Permission to write/modify project data', 'write', 'project', TRUE),
                    ('Read Projects', 'Permission to read project data', 'read', 'project', TRUE),
                    ('Write Object Storages', 'Permission to write/modify object storage', 'write', 'object_storage', TRUE),
                    ('Read Object Storages', 'Permission to read object storage', 'read', 'object_storage', TRUE),
                    ('Write Data Sources', 'Permission to write/modify data sources', 'write', 'data_source', TRUE),
                    ('Read Data Sources', 'Permission to read data sources', 'read', 'data_source', TRUE),
                    ('Write Records', 'Permission to write/modify records', 'write', 'record', TRUE),
                    ('Read Records', 'Permission to read records', 'read', 'record', TRUE),
                    ('Write Edges', 'Permission to write/modify edges', 'write', 'edge', TRUE),
                    ('Read Edges', 'Permission to read edges', 'read', 'edge', TRUE),
                    ('Write Files', 'Permission to write/modify files', 'write', 'file', TRUE),
                    ('Read Files', 'Permission to read files', 'read', 'file', TRUE),
                    ('Write Tags', 'Permission to write/modify tags', 'write', 'tag', TRUE),
                    ('Read Tags', 'Permission to read tags', 'read', 'tag', TRUE),
                    ('Write Classes', 'Permission to write/modify classes', 'write', 'class', TRUE),
                    ('Read Classes', 'Permission to read classes', 'read', 'class', TRUE),
                    ('Write Relationships', 'Permission to write/modify relationships', 'write', 'relationship', TRUE),
                    ('Read Relationships', 'Permission to read relationships', 'read', 'relationship', TRUE),
                    ('Write Users', 'Permission to write/modify users', 'write', 'user', TRUE),
                    ('Read Users', 'Permission to read users', 'read', 'user', TRUE),
                    ('Write Groups', 'Permission to write/modify groups', 'write', 'group', TRUE),
                    ('Read Groups', 'Permission to read groups', 'read', 'group', TRUE),
                    ('Write Organizations', 'Permission to write/modify organizations', 'write', 'organization', TRUE),
                    ('Read Organizations', 'Permission to read organizations', 'read', 'organization', TRUE),
                    ('Write Roles', 'Permission to write/modify roles', 'write', 'role', TRUE),
                    ('Read Roles', 'Permission to read roles', 'read', 'role', TRUE),
                    ('Write Permissions', 'Permission to write/modify permissions', 'write', 'permission', TRUE),
                    ('Read Permissions', 'Permission to read permissions', 'read', 'permission', TRUE),
                    ('Write Sensitivity Labels', 'Permission to write/modify sensitivity labels', 'write', 'sensitivity_label', TRUE),
                    ('Read Sensitivity Labels', 'Permission to read sensitivity labels', 'read', 'sensitivity_label', TRUE)
                ON CONFLICT (resource, action) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.permissions
                WHERE is_hardcoded = TRUE;
            ");
        }
    }
}
