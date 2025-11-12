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
            // STEP 1: Clear ALL existing role-permission associations
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.role_permissions;
            ");

            // ====================================================================
            // STEP 2: Remove ALL existing project member role assignments
            // ====================================================================
            migrationBuilder.Sql(@"
                UPDATE deeplynx.project_members
                SET role_id = NULL
                WHERE role_id IS NOT NULL;
            ");

            // ====================================================================
            // STEP 3: Delete ALL existing roles (for both organizations and projects)
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.roles
                WHERE (project_id IS NOT NULL OR organization_id IS NOT NULL)
                AND is_archived = false;
            ");

            // ====================================================================
            // STEP 4: Delete ALL existing permissions
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.permissions;
            ");

            // ====================================================================
            // STEP 5: Create default permissions
            // ====================================================================
            migrationBuilder.Sql(@"
                INSERT INTO deeplynx.permissions (name, description, resource, action, is_default, is_archived, last_updated_at)
                VALUES
                    -- Project permissions
                    ('Read Project', 'Permission to read project information', 'project', 'read', true, false, NOW()),
                    ('Write Project', 'Permission to modify project information', 'project', 'write', true, false, NOW()),
                    
                    -- Object Storage permissions
                    ('Read Object Storage', 'Permission to read object storage', 'object_storage', 'read', true, false, NOW()),
                    ('Write Object Storage', 'Permission to modify object storage', 'object_storage', 'write', true, false, NOW()),
                    
                    -- Data Source permissions
                    ('Read Data Source', 'Permission to read data sources', 'data_source', 'read', true, false, NOW()),
                    ('Write Data Source', 'Permission to modify data sources', 'data_source', 'write', true, false, NOW()),
                    
                    -- Record permissions
                    ('Read Record', 'Permission to read records', 'record', 'read', true, false, NOW()),
                    ('Write Record', 'Permission to modify records', 'record', 'write', true, false, NOW()),
                    
                    -- Edge permissions
                    ('Read Edge', 'Permission to read edges', 'edge', 'read', true, false, NOW()),
                    ('Write Edge', 'Permission to modify edges', 'edge', 'write', true, false, NOW()),
                    
                    -- File permissions
                    ('Read File', 'Permission to read files', 'file', 'read', true, false, NOW()),
                    ('Write File', 'Permission to modify files', 'file', 'write', true, false, NOW()),
                    
                    -- Tag permissions
                    ('Read Tag', 'Permission to read tags', 'tag', 'read', true, false, NOW()),
                    ('Write Tag', 'Permission to modify tags', 'tag', 'write', true, false, NOW()),
                    
                    -- Class permissions
                    ('Read Class', 'Permission to read classes', 'class', 'read', true, false, NOW()),
                    ('Write Class', 'Permission to modify classes', 'class', 'write', true, false, NOW()),
                    
                    -- Relationship permissions
                    ('Read Relationship', 'Permission to read relationships', 'relationship', 'read', true, false, NOW()),
                    ('Write Relationship', 'Permission to modify relationships', 'relationship', 'write', true, false, NOW()),
                    
                    -- User permissions
                    ('Read User', 'Permission to read user information', 'user', 'read', true, false, NOW()),
                    ('Write User', 'Permission to modify user information', 'user', 'write', true, false, NOW()),
                    
                    -- Group permissions
                    ('Read Group', 'Permission to read groups', 'group', 'read', true, false, NOW()),
                    ('Write Group', 'Permission to modify groups', 'group', 'write', true, false, NOW()),
                    
                    -- Organization permissions
                    ('Read Organization', 'Permission to read organization information', 'organization', 'read', true, false, NOW()),
                    ('Write Organization', 'Permission to modify organization information', 'organization', 'write', true, false, NOW()),
                    
                    -- Role permissions
                    ('Read Role', 'Permission to read roles', 'role', 'read', true, false, NOW()),
                    ('Write Role', 'Permission to modify roles', 'role', 'write', true, false, NOW()),
                    
                    -- Permission permissions
                    ('Read Permission', 'Permission to read permissions', 'permission', 'read', true, false, NOW()),
                    ('Write Permission', 'Permission to modify permissions', 'permission', 'write', true, false, NOW()),
                    
                    -- Sensitivity Label permissions
                    ('Read Sensitivity Label', 'Permission to read sensitivity labels', 'sensitivity_label', 'read', true, false, NOW()),
                    ('Write Sensitivity Label', 'Permission to modify sensitivity labels', 'sensitivity_label', 'write', true, false, NOW());
            ");

            // ====================================================================
            // STEP 6: Create Admin roles for ALL organizations
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
                WHERE o.is_archived = false;
            ");

            // ====================================================================
            // STEP 7: Create User roles for ALL organizations
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
                WHERE o.is_archived = false;
            ");

            // ====================================================================
            // STEP 8: Create Admin roles for ALL projects
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
                WHERE p.is_archived = false;
            ");

            // ====================================================================
            // STEP 9: Create User roles for ALL projects
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
                WHERE p.is_archived = false;
            ");

            // ====================================================================
            // STEP 10: Insert Admin permissions (for both orgs and projects)
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
            // STEP 11: Insert User permissions (for both orgs and projects)
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
            // STEP 12: Assign ALL existing project members to the Admin role
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

            // ====================================================================
            // STEP 13: Add all users to the default organization
            // Uses the organization where default_org = true
            // ====================================================================
            migrationBuilder.Sql(@"
                INSERT INTO deeplynx.organization_users (organization_id, user_id, is_org_admin)
                SELECT 
                    (SELECT id FROM deeplynx.organizations 
                     WHERE default_org = true 
                     AND is_archived = false 
                     LIMIT 1) as organization_id,
                    u.id as user_id,
                    false as is_org_admin
                FROM deeplynx.users u
                WHERE EXISTS (
                    SELECT 1 FROM deeplynx.organizations 
                    WHERE default_org = true 
                    AND is_archived = false
                )
                AND NOT EXISTS (
                    SELECT 1 FROM deeplynx.organization_users ou 
                    WHERE ou.user_id = u.id
                    AND ou.organization_id = (
                        SELECT id FROM deeplynx.organizations 
                        WHERE default_org = true 
                        AND is_archived = false 
                        LIMIT 1
                    )
                )
                ON CONFLICT (organization_id, user_id) DO NOTHING;
            ");
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ====================================================================
            // Rollback: Clear role_id from project_members
            // ====================================================================
            migrationBuilder.Sql(@"
                UPDATE deeplynx.project_members
                SET role_id = NULL
                WHERE role_id IS NOT NULL;
            ");

            // ====================================================================
            // Rollback: Remove ALL permission associations
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.role_permissions;
            ");

            // ====================================================================
            // Rollback: Remove ALL roles
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.roles
                WHERE (project_id IS NOT NULL OR organization_id IS NOT NULL)
                AND is_archived = false;
            ");

            // ====================================================================
            // Rollback: Remove ALL permissions
            // Note: This is destructive - you may want to comment this out
            // if you want to preserve custom permissions
            // ====================================================================
            migrationBuilder.Sql(@"
                DELETE FROM deeplynx.permissions;
            ");

        }
    }
}
