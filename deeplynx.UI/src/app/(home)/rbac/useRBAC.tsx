// useRBAC.tsx
import { useContext } from "react";
import { RBACContext } from "./RBACContext";
import { PERMISSIONS, ROLE_PERMISSIONS } from "./rbacConfig";

export function useRBAC() {
  const context = useContext(RBACContext);

  if (!context) {
    throw new Error("useRBAC must be used within RBACProvider");
  }

  const { user } = context;

  // Get user's role
  const getRole = (): string => {
    if (!user) return "viewer";
    if (user.isSysAdmin) return "sysAdmin";
    return user.role || "viewer"; // fallback to viewer
  };

  // Check if user has a specific permission
  const hasPermission = (permission: string): boolean => {
    if (!user || !user.isActive || user.isArchived) return false;

    const role = getRole();
    const permissions = ROLE_PERMISSIONS[role] || [];
    return permissions.includes(permission);
  };

  // Check if user has ANY of the permissions
  const hasAnyPermission = (permissions: string[]): boolean => {
    return permissions.some((permission) => hasPermission(permission));
  };

  // Check if user has ALL of the permissions
  const hasAllPermissions = (permissions: string[]): boolean => {
    return permissions.every((permission) => hasPermission(permission));
  };

  // Check if user has a specific role
  const hasRole = (role: string): boolean => {
    if (!user || !user.isActive || user.isArchived) return false;
    return getRole() === role;
  };

  // Check if user is active and not archived
  const isUserActive = (): boolean => {
    return user ? user.isActive && !user.isArchived : false;
  };

  return {
    user,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    isUserActive,
    role: getRole(),
    PERMISSIONS, // Export permissions for easy access
  };
}
