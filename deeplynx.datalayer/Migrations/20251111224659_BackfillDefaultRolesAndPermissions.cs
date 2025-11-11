using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class BackfillDefaultRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
         // ====================================================================
        // ORGANIZATIONS: STEP 1 - Create Admin roles
        // ====================================================================
        migrationBuilder.Sql(@"
            INSERT INTO deeplynx.roles (name, description, organization_id, last_updated_at, is_archived)
            SELECT 
                'Admin',
                'Organization administrator with full permissions',
                o.id,
                NOW(),
                false
            FROM deeplynx.organizations o
            WHERE o.is_archived = false
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.roles r 
                WHERE r.organization_id = o.id 
                AND r.name = 'Admin'
                AND r.is_archived = false
            );
        ");

        // ====================================================================
        // ORGANIZATIONS: STEP 2 - Create User roles
        // ====================================================================
        migrationBuilder.Sql(@"
            INSERT INTO deeplynx.roles (name, description, organization_id, last_updated_at, is_archived)
            SELECT 
                'User',
                'Standard organization user with limited permissions',
                o.id,
                NOW(),
                false
            FROM deeplynx.organizations o
            WHERE o.is_archived = false
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.roles r 
                WHERE r.organization_id = o.id 
                AND r.name = 'User'
                AND r.is_archived = false
            );
        ");

        // ====================================================================
        // PROJECTS: STEP 3 - Create Admin roles
        // ====================================================================
        migrationBuilder.Sql(@"
            INSERT INTO deeplynx.roles (name, description, project_id, last_updated_at, is_archived)
            SELECT 
                'Admin',
                'Project administrator with full permissions',
                p.id,
                NOW(),
                false
            FROM deeplynx.projects p
            WHERE p.is_archived = false
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.roles r 
                WHERE r.project_id = p.id 
                AND r.name = 'Admin'
                AND r.is_archived = false
            );
        ");

        // ====================================================================
        // PROJECTS: STEP 4 - Create User roles
        // ====================================================================
        migrationBuilder.Sql(@"
            INSERT INTO deeplynx.roles (name, description, project_id, last_updated_at, is_archived)
            SELECT 
                'User',
                'Standard project user with limited permissions',
                p.id,
                NOW(),
                false
            FROM deeplynx.projects p
            WHERE p.is_archived = false
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.roles r 
                WHERE r.project_id = p.id 
                AND r.name = 'User'
                AND r.is_archived = false
            );
        ");

        // ====================================================================
        // STEP 5: Clear existing permissions for Admin roles (organizations + projects)
        // ====================================================================
        migrationBuilder.Sql(@"
            DELETE FROM deeplynx.role_permissions
            WHERE role_id IN (
                SELECT id FROM deeplynx.roles 
                WHERE name = 'Admin' 
                AND (project_id IS NOT NULL OR organization_id IS NOT NULL)
                AND is_archived = false
            );
        ");

        // ====================================================================
        // STEP 6: Clear existing permissions for User roles (organizations + projects)
        // ====================================================================
        migrationBuilder.Sql(@"
            DELETE FROM deeplynx.role_permissions
            WHERE role_id IN (
                SELECT id FROM deeplynx.roles 
                WHERE name = 'User' 
                AND (project_id IS NOT NULL OR organization_id IS NOT NULL)
                AND is_archived = false
            );
        ");

        // ====================================================================
        // STEP 7: Insert Admin permissions (for both orgs and projects)
        // ====================================================================
        migrationBuilder.Sql(@"
            INSERT INTO deeplynx.role_permissions (permission_id, role_id)
            SELECT DISTINCT p.id, r.id
            FROM deeplynx.roles r
            CROSS JOIN deeplynx.permissions p
            WHERE r.name = 'Admin'
            AND (r.project_id IS NOT NULL OR r.organization_id IS NOT NULL)
            AND r.is_archived = false
            AND p.is_archived = false
            AND p.resource IS NOT NULL
            AND (
                (p.resource = 'project' AND p.action IN ('read', 'write')) OR
                (p.resource = 'object_storage' AND p.action IN ('read', 'write')) OR
                (p.resource = 'data_source' AND p.action IN ('read', 'write')) OR
                (p.resource = 'record' AND p.action IN ('read', 'write')) OR
                (p.resource = 'edge' AND p.action IN ('read', 'write')) OR
                (p.resource = 'file' AND p.action IN ('read', 'write')) OR
                (p.resource = 'tag' AND p.action IN ('read', 'write')) OR
                (p.resource = 'class' AND p.action IN ('read', 'write')) OR
                (p.resource = 'relationship' AND p.action IN ('read', 'write')) OR
                (p.resource = 'user' AND p.action IN ('read', 'write')) OR
                (p.resource = 'group' AND p.action IN ('read', 'write')) OR
                (p.resource = 'organization' AND p.action = 'read') OR
                (p.resource = 'role' AND p.action IN ('read', 'write')) OR
                (p.resource = 'permission' AND p.action IN ('read', 'write')) OR
                (p.resource = 'sensitivity_label' AND p.action IN ('read', 'write'))
            );
        ");

        // ====================================================================
        // STEP 8: Insert User permissions (for both orgs and projects)
        // ====================================================================
        migrationBuilder.Sql(@"
            INSERT INTO deeplynx.role_permissions (permission_id, role_id)
            SELECT DISTINCT p.id, r.id
            FROM deeplynx.roles r
            CROSS JOIN deeplynx.permissions p
            WHERE r.name = 'User'
            AND (r.project_id IS NOT NULL OR r.organization_id IS NOT NULL)
            AND r.is_archived = false
            AND p.is_archived = false
            AND p.resource IS NOT NULL
            AND (
                (p.resource = 'project' AND p.action = 'read') OR
                (p.resource = 'object_storage' AND p.action = 'read') OR
                (p.resource = 'data_source' AND p.action IN ('read', 'write')) OR
                (p.resource = 'record' AND p.action IN ('read', 'write')) OR
                (p.resource = 'edge' AND p.action IN ('read', 'write')) OR
                (p.resource = 'file' AND p.action IN ('read', 'write')) OR
                (p.resource = 'tag' AND p.action IN ('read', 'write')) OR
                (p.resource = 'class' AND p.action IN ('read', 'write')) OR
                (p.resource = 'relationship' AND p.action IN ('read', 'write')) OR
                (p.resource = 'user' AND p.action = 'read') OR
                (p.resource = 'group' AND p.action = 'read') OR
                (p.resource = 'organization' AND p.action = 'read') OR
                (p.resource = 'role' AND p.action = 'read') OR
                (p.resource = 'permission' AND p.action = 'read') OR
                (p.resource = 'sensitivity_label' AND p.action = 'read')
            );
        ");

        // ====================================================================
        // STEP 9: Update existing project members to have the Admin role
        // ====================================================================
        migrationBuilder.Sql(@"
            UPDATE deeplynx.project_members pm
            SET role_id = r.id
            FROM deeplynx.roles r
            WHERE pm.role_id IS NULL
            AND r.project_id = pm.project_id
            AND r.name = 'Admin'
            AND r.is_archived = false;
        ");
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ====================================================================
            // Rollback: Clear role_id from project_members
            // ====================================================================
            migrationBuilder.Sql(@"
            UPDATE deeplynx.project_members pm
            SET role_id = NULL
            WHERE role_id IN (
                SELECT id FROM deeplynx.roles 
                WHERE name = 'Admin'
                AND project_id IS NOT NULL
            );
        ");

            // ====================================================================
            // Rollback: Remove permission associations
            // ====================================================================
            migrationBuilder.Sql(@"
            DELETE FROM deeplynx.role_permissions
            WHERE role_id IN (
                SELECT id FROM deeplynx.roles 
                WHERE name IN ('Admin', 'User')
                AND (project_id IS NOT NULL OR organization_id IS NOT NULL)
                AND is_archived = false
            );
        ");

            // ====================================================================
            // Rollback: Remove roles (only if they have no members)
            // ====================================================================
            migrationBuilder.Sql(@"
            DELETE FROM deeplynx.roles
            WHERE name IN ('Admin', 'User')
            AND organization_id IS NOT NULL
            AND is_archived = false;

            DELETE FROM deeplynx.roles
            WHERE name IN ('Admin', 'User')
            AND project_id IS NOT NULL
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.project_members pm 
                WHERE pm.role_id = roles.id
            );
        ");

        }
    }
}
