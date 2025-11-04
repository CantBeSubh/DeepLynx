// rbacConfig.ts

// ============================================
// Define all your permissions as constants
// ============================================
export const PERMISSIONS = {
  // SysAdmin settings page
  SYSADMIN_PAGE: 'sysadmin_page',
  
  // User Management
  VIEW_USERS: 'view_users',
  CREATE_USER: 'create_user',
  EDIT_USER: 'edit_user',
  DELETE_USER: 'delete_user',
  ARCHIVE_USER: 'archive_user',
  
  // System Settings
  VIEW_SETTINGS: 'view_settings',
  EDIT_SETTINGS: 'edit_settings',
  
  // Reports
  VIEW_REPORTS: 'view_reports',
  EXPORT_REPORTS: 'export_reports',
  
  // Dashboard
  VIEW_DASHBOARD: 'view_dashboard',
  VIEW_ANALYTICS: 'view_analytics',
} as const;

// ============================================
// Map each role to its permissions
// ============================================
export const ROLE_PERMISSIONS: Record<string, string[]> = {
  // System Admin gets everything
  sysAdmin: Object.values(PERMISSIONS),
  
  // Manager role
  manager: [
    PERMISSIONS.VIEW_USERS,
    PERMISSIONS.EDIT_USER,
    PERMISSIONS.VIEW_SETTINGS,
    PERMISSIONS.VIEW_REPORTS,
    PERMISSIONS.EXPORT_REPORTS,
    PERMISSIONS.VIEW_DASHBOARD,
    PERMISSIONS.VIEW_ANALYTICS,
  ],
  
  // Viewer role
  viewer: [
    PERMISSIONS.VIEW_USERS,
    PERMISSIONS.VIEW_DASHBOARD,
    PERMISSIONS.VIEW_REPORTS,
  ],
};