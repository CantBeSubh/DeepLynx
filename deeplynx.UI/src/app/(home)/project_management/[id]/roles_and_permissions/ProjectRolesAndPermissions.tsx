// src/app/(home)/project_management/[id]/roles_and_permissions/ProjectRolesAndPermissions.tsx
"use client";

import React, { useEffect, useRef, useState } from "react";
import toast from "react-hot-toast";

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";

import // archiveRole,
// createRole,
// getProjectRolePermissions,
// setPermissionsForRole,
// updateRole,
"@/app/lib/client_service/role_services.client";

import {
  PermissionResponseDto,
  RoleResponseDto,
} from "@/app/(home)/types/responseDTOs";

import {
  BuildingOfficeIcon,
  CheckIcon,
  ExclamationCircleIcon,
  LockClosedIcon,
  ShieldCheckIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import { getPermissionsForRole } from "@/app/lib/client_service/permission_services.client";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

interface ProjectRolesAndPermissionsProps {
  initialRoles: RoleResponseDto[];
  initialPermissions: PermissionResponseDto[];
  projectId: number;
  rolesLocked?: boolean;
}

interface PermissionCategory {
  id: string;
  label: string;
  permissions: PermissionResponseDto[];
}

/* -------------------------------------------------------------------------- */
/*                           ProjectRolesAndPermissions                       */
/* -------------------------------------------------------------------------- */

const ProjectRolesAndPermissions = ({
  initialRoles,
  initialPermissions,
  projectId,
  rolesLocked = false,
}: ProjectRolesAndPermissionsProps) => {
  /* ------------------------------------------------------------------------ */
  /*                               Core State                                */
  /* ------------------------------------------------------------------------ */

  // For this release: only standard roles (Admin, User, Viewer)
  const standardInitialRoles = initialRoles.filter((role) =>
    ["Admin", "User", "Viewer"].includes(role.name)
  );
  console.log("Roles: ", initialRoles);
  console.log("Permissions: ", initialPermissions);
  console.log("project: ", projectId);

  const [activeLayout, setActiveLayout] = useState<"split-view" | "matrix">(
    "split-view"
  );
  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(
    standardInitialRoles[0]?.id || null
  );
  const [roles] = useState<RoleResponseDto[]>(standardInitialRoles);
  const [permissions] = useState<PermissionResponseDto[]>(initialPermissions);

  const [rolePermissions, setRolePermissions] = useState<
    Record<number, PermissionResponseDto[]>
  >({});
  const [isLoadingPermissions, setIsLoadingPermissions] = useState(false);
  const [initialLoadComplete, setInitialLoadComplete] = useState(false);

  const { organization } = useOrganizationSession();

  /* ------------------------------------------------------------------------ */
  /*                 Permission grouping & derived helpers                    */
  /* ------------------------------------------------------------------------ */

  const groupPermissionsByResource = (): PermissionCategory[] => {
    const grouped = permissions.reduce((acc, perm) => {
      const resource = perm.resource || "General";
      if (!acc[resource]) acc[resource] = [];
      acc[resource].push(perm);
      return acc;
    }, {} as Record<string, PermissionResponseDto[]>);

    return Object.entries(grouped).map(([resource, perms]) => ({
      id: resource.toLowerCase().replace(/\s+/g, "-"),
      label: resource,
      permissions: perms,
    }));
  };

  const permissionCategories = groupPermissionsByResource();
  const currentRole = roles.find((r) => r.id === selectedRoleId);

  const roleHasPermission = (roleId: number, permissionId: number): boolean =>
    rolePermissions[roleId]?.some((p) => p.id === permissionId) || false;

  const isStandardRole = (role: RoleResponseDto): boolean =>
    role.name === "Admin" || role.name === "User" || role.name === "Viewer";

  const isOrganizationRole = (role: RoleResponseDto): boolean =>
    role.organizationId != null && role.projectId == null;

  const isProjectRole = (role: RoleResponseDto): boolean =>
    role.projectId != null && role.projectId === projectId;

  const getRoleSource = (role: RoleResponseDto): string => {
    if (isStandardRole(role)) return "System";
    if (isOrganizationRole(role)) return "Organization";
    if (isProjectRole(role)) return "Project";
    return "Unknown";
  };

  // For this release: roles are read-only, permissions read-only
  const canEditRole = (_role: RoleResponseDto): boolean => false;
  const canEditPermissions = (_role: RoleResponseDto): boolean => false;

  /* ------------------------------------------------------------------------ */
  /*                       Permissions Fetching (per role)                    */
  /* ------------------------------------------------------------------------ */

  const fetchRolePermissions = async (roleId: number) => {
    if (rolePermissions[roleId]) return;
    if (!organization?.organizationId) return;

    setIsLoadingPermissions(true);
    try {
      const perms = await getPermissionsForRole(
        Number(organization.organizationId),
        projectId,
        roleId
      );
      setRolePermissions((prev) => ({
        ...prev,
        [roleId]: perms,
      }));
    } catch (error) {
      console.error(`Error fetching permissions for role ${roleId}:`, error);
      toast.error("Failed to load role permissions");
    } finally {
      setIsLoadingPermissions(false);
    }
  };

  const fetchAllRolePermissions = async () => {
    if (!organization?.organizationId) return;

    setIsLoadingPermissions(true);
    try {
      const orgId = Number(organization.organizationId);

      const promises = roles.map((role) =>
        getPermissionsForRole(orgId, projectId, role.id)
          .then((perms) => ({ roleId: role.id, perms }))
          .catch((error) => {
            console.error(
              `Error fetching permissions for role ${role.id}:`,
              error
            );
            return { roleId: role.id, perms: [] as PermissionResponseDto[] };
          })
      );

      const results = await Promise.all(promises);

      const newRolePermissions: Record<number, PermissionResponseDto[]> = {};
      results.forEach(({ roleId, perms }) => {
        newRolePermissions[roleId] = perms;
      });

      setRolePermissions(newRolePermissions);
      setInitialLoadComplete(true);
    } catch (error) {
      console.error("Error fetching all role permissions:", error);
      toast.error("Failed to load permissions");
    } finally {
      setIsLoadingPermissions(false);
    }
  };

  useEffect(() => {
    if (!initialLoadComplete && roles.length > 0) {
      fetchAllRolePermissions();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [roles, initialLoadComplete]);

  useEffect(() => {
    if (selectedRoleId && initialLoadComplete) {
      fetchRolePermissions(selectedRoleId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedRoleId, initialLoadComplete]);

  const handleRoleSelection = (roleId: number) => {
    setSelectedRoleId(roleId);
  };

  /* ------------------------------------------------------------------------ */
  /*                               Layout: Split View                         */
  /* ------------------------------------------------------------------------ */

  const SplitViewLayout = () => (
    <div className="flex gap-6" style={{ height: "calc(100vh - 28rem)" }}>
      {/* Roles List */}
      <div className="w-80 flex-shrink-0">
        <div className="card bg-base-100 shadow-xl h-full flex flex-col border-2 border-primary">
          <div className="card-body p-0">
            <div className="px-4 py-3 border-base-300">
              <h2 className="card-title text-base">Roles</h2>
              <p className="text-xs text-base-content/60 mt-1">
                {roles.length} total
              </p>
            </div>
            <div className="divider px-3"></div>
            <div className="flex-1 overflow-y-auto">
              {roles.map((role) => (
                <button
                  key={role.id}
                  onClick={() => handleRoleSelection(role.id)}
                  className={`w-full px-4 py-3 text-left border-b border-base-300 transition-colors ${
                    selectedRoleId === role.id
                      ? "bg-primary/10 border-l-4 border-l-primary"
                      : ""
                  } hover:bg-base-200 cursor-pointer`}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <ShieldCheckIcon
                        className={`w-4 h-4 ${
                          selectedRoleId === role.id
                            ? "text-primary"
                            : "text-base-content/40"
                        }`}
                      />
                      <span className="font-medium text-sm">{role.name}</span>
                    </div>
                    {isStandardRole(role) && (
                      <div className="badge badge-info badge-sm">STD</div>
                    )}
                    {isOrganizationRole(role) && (
                      <div className="badge badge-secondary badge-sm flex gap-1">
                        <BuildingOfficeIcon className="w-3 h-3" />
                        ORG
                      </div>
                    )}
                    {isProjectRole(role) && (
                      <div className="badge badge-primary badge-sm">PRJ</div>
                    )}
                  </div>
                  {role.description && (
                    <p className="text-xs text-base-content/60 mt-1 ml-6 truncate">
                      {role.description}
                    </p>
                  )}
                  <p className="text-xs text-base-content/50 mt-1 ml-6">
                    Source: {getRoleSource(role)}
                  </p>
                </button>
              ))}
            </div>
            {/* No "New Project Role" button in this release */}
          </div>
        </div>
      </div>

      {/* Role details & permissions */}
      <div className="flex-1 card bg-base-100 shadow-xl flex flex-col overflow-hidden border-2 border-primary">
        {currentRole ? (
          <>
            {/* Role header */}
            <div className="px-6 py-4 border-base-300 flex-shrink-0">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-3">
                    <h2 className="card-title">{currentRole.name}</h2>
                    {isStandardRole(currentRole) && (
                      <div className="badge badge-info">Standard Role</div>
                    )}
                    {isOrganizationRole(currentRole) && (
                      <div className="badge badge-secondary gap-1">
                        <BuildingOfficeIcon className="w-4 h-4" />
                        Organization Role
                      </div>
                    )}
                    {isProjectRole(currentRole) && (
                      <div className="badge badge-primary">Project Role</div>
                    )}
                  </div>
                  {currentRole.description && (
                    <p className="text-sm text-base-content/70 mt-1">
                      {currentRole.description}
                    </p>
                  )}
                  <p className="text-xs text-base-content/60 mt-2">
                    Source: {getRoleSource(currentRole)} • Last updated:{" "}
                    {new Date(currentRole.lastUpdatedAt).toLocaleDateString()}
                  </p>
                </div>
                {/* No edit/delete actions in this release */}
              </div>
            </div>

            {/* Info about non-editable perms */}
            <div className="p-3">
              <div className="alert alert-info py-2">
                <ExclamationCircleIcon className="w-5 h-5 flex-shrink-0" />
                <span className="text-sm">
                  Role permissions are read-only in this release. Standard roles
                  are defined by the system and cannot be modified here.
                </span>
              </div>
            </div>

            <div className="divider px-3"></div>

            {/* Permissions list for selected role */}
            <div className="flex-1 overflow-y-auto p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold">Permissions</h3>
                {/* No "Edit Permissions" button in this release */}
              </div>

              {isLoadingPermissions ? (
                <div className="flex items-center justify-center py-12">
                  <span className="loading loading-spinner loading-lg text-primary"></span>
                </div>
              ) : permissionCategories.length === 0 ? (
                <div className="alert">
                  <span>No permissions available.</span>
                </div>
              ) : (
                <div className="space-y-4">
                  {permissionCategories.map((category) => (
                    <div key={category.id} className="card bg-base-200/25">
                      <div className="card-body p-4">
                        <h4 className="card-title text-sm mb-3">
                          {category.label}
                        </h4>
                        <div className="grid grid-cols-2 gap-3">
                          {category.permissions.map((perm) => {
                            const hasPermission = roleHasPermission(
                              currentRole.id,
                              Number(perm.id)
                            );

                            return (
                              <label
                                key={perm.id}
                                className="label justify-start gap-2 cursor-default"
                                title={perm.description || perm.name}
                              >
                                <input
                                  type="checkbox"
                                  checked={hasPermission}
                                  readOnly
                                  className="checkbox checkbox-primary checkbox-sm"
                                />
                                <span className="label-text">{perm.name}</span>
                              </label>
                            );
                          })}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </>
        ) : (
          <div className="flex items-center justify-center h-full">
            <p className="text-base-content/60">
              Select a role to view details
            </p>
          </div>
        )}
      </div>
    </div>
  );

  /* ------------------------------------------------------------------------ */
  /*                               Layout: Matrix View                        */
  /* ------------------------------------------------------------------------ */

  const tableContainerRef = useRef<HTMLDivElement>(null);

  const MatrixViewLayout = () => (
    <div style={{ height: "calc(100vh - 28rem)" }}>
      <div className="card bg-base-100 shadow-xl h-full flex flex-col overflow-hidden">
        <div className="px-6 py-3 border-b border-base-300 flex items-center justify-between">
          <h3 className="text-sm font-semibold">Permission Matrix</h3>
          {/* No edit matrix button in this release */}
        </div>

        {isLoadingPermissions && !initialLoadComplete ? (
          <div className="flex-1 flex items-center justify-center">
            <div className="text-center">
              <span className="loading loading-spinner loading-lg text-primary"></span>
              <p className="mt-4 text-base-content/60">
                Loading permissions...
              </p>
            </div>
          </div>
        ) : (
          <div ref={tableContainerRef} className="flex-1 overflow-auto">
            <table className="table">
              <thead className="sticky top-0 z-20 bg-base-100">
                <tr>
                  <th className="sticky left-0 bg-base-200 z-30">Permission</th>
                  {roles.map((role) => (
                    <th key={role.id} className="text-center">
                      <div className="flex flex-col items-center gap-1">
                        <div className="flex items-center gap-2">
                          <ShieldCheckIcon className="w-4 h-4 text-primary" />
                          <span className="font-medium">{role.name}</span>
                          {isStandardRole(role) && (
                            <div className="badge badge-info badge-xs">STD</div>
                          )}
                          {isOrganizationRole(role) && (
                            <div className="badge badge-secondary badge-xs flex gap-1">
                              <BuildingOfficeIcon className="w-3 h-3" />
                              ORG
                            </div>
                          )}
                          {isProjectRole(role) && (
                            <div className="badge badge-primary badge-xs">
                              PRJ
                            </div>
                          )}
                        </div>
                        {role.description && (
                          <span className="text-xs text-base-content/60 font-normal">
                            {role.description}
                          </span>
                        )}
                        <span className="text-xs text-base-content/50 font-normal">
                          {getRoleSource(role)}
                        </span>
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {permissionCategories.map((category) => (
                  <React.Fragment key={category.id}>
                    <tr className="bg-base-200">
                      <td
                        colSpan={roles.length + 2}
                        className="font-semibold text-sm sticky left-0"
                      >
                        {category.label}
                      </td>
                    </tr>
                    {category.permissions.map((perm) => (
                      <tr key={perm.id} className="hover">
                        <td className="sticky left-0 z-10 bg-base-100">
                          <div className="flex flex-col">
                            <span className="font-medium text-sm">
                              {perm.name}
                            </span>
                            {perm.description && (
                              <span className="text-xs text-base-content/60">
                                {perm.description}
                              </span>
                            )}
                            <span className="text-xs text-base-content/50 mt-1">
                              Action: {perm.action}
                            </span>
                          </div>
                        </td>
                        {roles.map((role) => {
                          const hasPermission = roleHasPermission(
                            role.id,
                            Number(perm.id)
                          );

                          return (
                            <td key={role.id} className="text-center">
                              <div
                                className="inline-block cursor-default"
                                title={
                                  hasPermission
                                    ? "Has permission"
                                    : "No permission"
                                }
                              >
                                {hasPermission ? (
                                  <CheckIcon className="size-8 mx-auto text-success" />
                                ) : (
                                  <XMarkIcon className="size-8 mx-auto text-base-300" />
                                )}
                              </div>
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </React.Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );

  /* ------------------------------------------------------------------------ */
  /*                                   Render                                 */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6 mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <h1 className="text-2xl font-bold">Project Roles & Permissions</h1>
        </div>
        <p className="text-base-content/70">
          View project-level roles and their permissions. Standard roles are
          defined by the system and are read-only in this release.
        </p>
      </div>

      {/* Global lock notice (still useful context) */}
      {rolesLocked && (
        <div className="alert alert-warning mb-6">
          <LockClosedIcon className="w-6 h-6" />
          <div>
            <h3 className="font-bold">Roles Locked by Organization</h3>
            <div className="text-sm">
              Project-level role creation and permission modification is
              disabled. Contact your organization administrator to unlock.
            </div>
          </div>
        </div>
      )}

      {/* Layout toggle */}
      <div className="mb-6">
        <label className="label">
          <span className="label-text font-medium">View Layout:</span>
        </label>
        <div className="btn-group">
          <button
            onClick={() => setActiveLayout("split-view")}
            className={`btn border-2 border-primary mr-3 ${
              activeLayout === "split-view" ? "btn-primary" : "btn-ghost"
            }`}
          >
            Split View
          </button>
          <button
            onClick={() => setActiveLayout("matrix")}
            className={`btn border-2 border-primary ${
              activeLayout === "matrix" ? "btn-primary" : "btn-ghost"
            }`}
          >
            Matrix View
          </button>
        </div>
      </div>

      {/* Layouts */}
      {activeLayout === "split-view" && <SplitViewLayout />}
      {activeLayout === "matrix" && <MatrixViewLayout />}
    </div>
  );
};

export default ProjectRolesAndPermissions;
