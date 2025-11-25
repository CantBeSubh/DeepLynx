using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CleanDefaultRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ====================================================================
            // STEP 1: Clear role_permissions table
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.role_permissions;
                ");

            // ====================================================================
            // STEP 2: Add permissions to Admin roles
            // ====================================================================
            migrationBuilder.Sql(@"
                INSERT INTO deeplynx.role_permissions (role_id, permission_id)
                SELECT 
                    r.id AS role_id,
                    p.id AS permission_id
                FROM 
                    deeplynx.roles r
                CROSS JOIN 
                    deeplynx.permissions p
                WHERE 
                    r.name = 'Admin'
                    AND r.is_archived = FALSE
                    AND p.is_default = TRUE
                    AND p.is_archived = FALSE
                    AND p.project_id IS NULL
                    AND p.organization_id IS NULL
                    AND p.label_id IS NULL
                    AND (
                        (p.resource = 'project' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'object_storage' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'data_source' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'record' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'edge' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'file' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'tag' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'class' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'relationship' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'user' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'group' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'organization' AND p.action = 'read')
                        OR (p.resource = 'role' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'permission' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'sensitivity_label' AND p.action IN ('read', 'write'))
                    );
            ");

            // ====================================================================
            // STEP 3: Add permissions to User roles
            // ====================================================================
            migrationBuilder.Sql(@"
                INSERT INTO deeplynx.role_permissions (role_id, permission_id)
                SELECT 
                    r.id AS role_id,
                    p.id AS permission_id
                FROM 
                    deeplynx.roles r
                CROSS JOIN 
                    deeplynx.permissions p
                WHERE 
                    r.name = 'User'
                    AND r.is_archived = FALSE
                    AND p.is_default = TRUE
                    AND p.is_archived = FALSE
                    AND p.project_id IS NULL
                    AND p.organization_id IS NULL
                    AND p.label_id IS NULL
                    AND (
                        (p.resource = 'project' AND p.action = 'read')
                        OR (p.resource = 'object_storage' AND p.action = 'read')
                        OR (p.resource = 'data_source' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'record' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'edge' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'file' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'tag' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'class' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'relationship' AND p.action IN ('read', 'write'))
                        OR (p.resource = 'user' AND p.action = 'read')
                        OR (p.resource = 'group' AND p.action = 'read')
                        OR (p.resource = 'organization' AND p.action = 'read')
                        OR (p.resource = 'role' AND p.action = 'read')
                        OR (p.resource = 'permission' AND p.action = 'read')
                        OR (p.resource = 'sensitivity_label' AND p.action = 'read')
                    );
            ");
            
            // ====================================================================
            // STEP 4: Delete unwanted non-default permissions
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.permissions
                WHERE organization_id IS NOT NULL
                   OR project_id IS NOT NULL
                   ");
            
            // ====================================================================
            // STEP 5: Add constraint that id default is true, it can't have a project, org, or label ID
            // ====================================================================
            migrationBuilder.AddCheckConstraint(
                name: "chk_default_permissions_no_org_project_label",
                schema: "deeplynx",
                table: "permissions",
                sql: "is_default = false OR (organization_id IS NULL AND project_id IS NULL AND label_id IS NULL)"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_default_permissions_no_org_project_label",
                schema: "deeplynx",
                table: "permissions");
        }
    }
}
