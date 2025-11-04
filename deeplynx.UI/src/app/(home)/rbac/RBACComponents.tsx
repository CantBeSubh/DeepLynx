// src/app/(home)/rbac/RBACComponents.tsx
"use client";

import React, { ReactNode } from "react";
import { useRBAC } from "./useRBAC";

// ============================================
// CanAccess Component
// ============================================
interface CanAccessProps {
  permission?: string;
  permissions?: string[];
  requireAll?: boolean;
  fallback?: ReactNode;
  children: ReactNode;
}

/**
 * For UI Elements & Features
 * Use this for small pieces of UI like buttons, menu items, sections, or features within a page.
 * USE CASE:
 *
 * - Show/hide buttons (Create, Edit, Delete)
 * - Show/hide menu items in navigation
 * - Show/hide specific sections or cards on a page
 * - Enable/disable form fields
 * - Show/hide table columns or actions
 *
 *
 * Conditionally render children based on user permissions
 *
 * @example
 * // Single permission
 * <CanAccess permission={PERMISSIONS.CREATE_USER}>
 *   <button>Create User</button>
 * </CanAccess>
 *
 * @example
 * // Multiple permissions (any of them)
 * <CanAccess permissions={[PERMISSIONS.EDIT_USER, PERMISSIONS.DELETE_USER]}>
 *   <button>Manage User</button>
 * </CanAccess>
 *
 * @example
 * // Require all permissions
 * <CanAccess
 *   permissions={[PERMISSIONS.VIEW_ANALYTICS, PERMISSIONS.EXPORT_REPORTS]}
 *   requireAll={true}
 * >
 *   <button>Export Analytics</button>
 * </CanAccess>
 *
 * @example
 * // With fallback
 * <CanAccess
 *   permission={PERMISSIONS.VIEW_SETTINGS}
 *   fallback={<div>You don't have access</div>}
 * >
 *   <SettingsPanel />
 * </CanAccess>
 */
export function CanAccess({
  permission,
  permissions,
  requireAll = false,
  fallback = null,
  children,
}: CanAccessProps) {
  const { hasPermission, hasAnyPermission, hasAllPermissions } = useRBAC();

  let canAccess = false;

  if (permission) {
    canAccess = hasPermission(permission);
  } else if (permissions) {
    canAccess = requireAll
      ? hasAllPermissions(permissions)
      : hasAnyPermission(permissions);
  }

  return canAccess ? <>{children}</> : <>{fallback}</>;
}

// ============================================
// RestrictedRoute Component
// ============================================
interface RestrictedRouteProps {
  permission: string;
  fallback?: ReactNode;
  children: ReactNode;
}

/**
 * For Entire Pages
 * Use this to protect whole pages or routes. If user doesn't have permission, they see "Access Denied" instead of the page content.
 * Use cases:
 *
 * - Admin-only pages
 * - Settings pages
 * - Reports pages
 * - Any full page that requires specific permissions
 *
 * @example
 * <RestrictedRoute permission={PERMISSIONS.VIEW_USERS}>
 *   <UserManagementPage />
 * </RestrictedRoute>
 *
 * @example
 * // With custom fallback
 * <RestrictedRoute
 *   permission={PERMISSIONS.VIEW_SETTINGS}
 *   fallback={<CustomAccessDenied />}
 * >
 *   <SettingsPage />
 * </RestrictedRoute>
 */
export function RestrictedRoute({
  permission,
  fallback,
  children,
}: RestrictedRouteProps) {
  const { hasPermission } = useRBAC();

  if (!hasPermission(permission)) {
    if (fallback) {
      return <>{fallback}</>;
    }

    return (
      <div className="flex items-center justify-center min-h-screen p-8">
        <div className="text-center max-w-md">
          <div className="text-6xl mb-4">🔒</div>
          <h2 className="text-2xl font-bold text-error mb-4">Access Denied</h2>
          <p className="text-base-content/70">
            You don't have permission to access this page.
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}

// ============================================
// RoleGate Component (Bonus)
// ============================================
interface RoleGateProps {
  role: string;
  fallback?: ReactNode;
  children: ReactNode;
}

/**
 *  For Role-Based Rendering
 * Use this when you need to check the user's role rather than individual permissions. This is less flexible but simpler when you have clear role distinctions.
 * Use cases:
 *
 * - Admin-only dashboards
 * - Different UI layouts for different roles
 * - Role-specific welcome messages
 * - When you need simple "is this user an admin?" checks
 *
 * @example
 * <RoleGate role="sysAdmin">
 *   <AdminPanel />
 * </RoleGate>
 *
 * @example
 * <div>
 *    <RoleGate role="sysAdmin">
 *      <AdminDashboard />
 *    </RoleGate>
 *
 *    <RoleGate role="manager">
 *       <ManagerDashboard />
 *    </RoleGate>
 *
 *    <RoleGate role="viewer">
 *     <ViewerDashboard />
 *    </RoleGate>
 * </div>
 */
export function RoleGate({ role, fallback = null, children }: RoleGateProps) {
  const { hasRole } = useRBAC();

  return hasRole(role) ? <>{children}</> : <>{fallback}</>;
}
