// src/app/(home)/rbac/rbacConfig.ts

// ============================================
// Define all your permissions as constants
// ============================================
export const PERMISSIONS = {
  // SysAdmin settings page
  SYSADMIN_PAGE: "sysadmin_page",

  // Organization level
  ORGANIZATION_PORTAL: "orgadmin_page",

  // Project level
  PROJECT_PORTAL: "projectadmin_page",
} as const;

// ============================================
// Role types
// ============================================
export type CoreRBACRole = "sysAdmin" | "orgAdmin" | "projectAdmin";
export type AnyRBACRole = CoreRBACRole | "viewer";

// ============================================
// Map each role to its permissions
// ============================================
export const ROLE_PERMISSIONS: Record<AnyRBACRole, string[]> = {
  // System Admin gets everything
  sysAdmin: Object.values(PERMISSIONS),

  // Organization Admin gets org + project portals
  orgAdmin: [
    PERMISSIONS.ORGANIZATION_PORTAL, 
    PERMISSIONS.PROJECT_PORTAL
  ],

  // Project Admin gets only the project portal
  projectAdmin: [
    PERMISSIONS.PROJECT_PORTAL
  ],

  // Viewer / regular user has no portal access
  viewer: [],
};
