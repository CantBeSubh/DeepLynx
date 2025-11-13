// src/app/(home)/organization_management/roles_and_permissions/RolesAndPermissions.tsx

import {
  CheckIcon,
  ExclamationCircleIcon,
  LockClosedIcon,
  LockOpenIcon,
  PencilIcon,
  PlusIcon,
  ShieldCheckIcon,
  TrashIcon,
  UsersIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import React, { useEffect, useState } from "react";
import {
  PermissionResponseDto,
  RoleResponseDto,
} from "../../types/responseDTOs";
import { getPermissionsByRole } from "@/app/lib/role_services.client";

interface RolesAndPermissionsProps {
  initialRoles: RoleResponseDto[];
  initialPermissions: PermissionResponseDto[];
}

interface PermissionCategory {
  id: string;
  label: string;
  permissions: PermissionResponseDto[];
}

const RolesAndPermissions = ({
  initialRoles,
  initialPermissions,
}: RolesAndPermissionsProps) => {
  const [activeLayout, setActiveLayout] = useState("split-view");
  const [rolesLocked, setRolesLocked] = useState(false);
  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(
    initialRoles[0]?.id || null
  );
  const [roles, setRoles] = useState(initialRoles);
  const [permissions, setPermissions] = useState(initialPermissions);
  const [rolePermissions, setRolePermissions] = useState<
    Record<number, PermissionResponseDto[]>
  >({});

  // Group permissions by resource (or use a default category)
  const groupPermissionsByResource = (): PermissionCategory[] => {
    const grouped = permissions.reduce((acc, perm) => {
      const resource = perm.resource || "General";
      if (!acc[resource]) {
        acc[resource] = [];
      }
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

  // Fetch permissions for a role
  const fetchRolePermissions = async (roleId: number) => {
    if (rolePermissions[roleId]) return; // Already fetched

    try {
      const perms = await getPermissionsByRole(roleId);
      setRolePermissions((prev) => ({
        ...prev,
        [roleId]: perms,
      }));
    } catch (error) {
      console.error(`Error fetching permissions for role ${roleId}:`, error);
    }
  };

  // Fetch permissions when a role is selected
  useEffect(() => {
    if (selectedRoleId) {
      fetchRolePermissions(selectedRoleId);
    }
  }, [selectedRoleId]);

  // Check if a role has a specific permission
  const roleHasPermission = (roleId: number, permissionId: number): boolean => {
    return rolePermissions[roleId]?.some((p) => p.id === permissionId) || false;
  };

  // Determine if role is "standard" (you might need to add an isDefault field to RoleResponseDto)
  const isStandardRole = (role: RoleResponseDto): boolean => {
    // You might want to add a field to your DTO to indicate this
    // For now, we'll use a simple check - adjust based on your needs
    return (
      role.name === "Admin" || role.name === "User" || role.name === "Viewer"
    );
  };

  // Layout 1: Split View (Master-Detail)
  const SplitViewLayout = () => (
    <div className="flex gap-6 h-[calc(100vh-12rem)]">
      {/* Left Sidebar - Roles List */}
      <div className="w-80 flex-shrink-0">
        <div className="bg-white rounded-lg border border-gray-200 h-full flex flex-col">
          <div className="px-4 py-3 border-b border-gray-200">
            <h2 className="font-semibold text-gray-900">Roles</h2>
            <p className="text-xs text-gray-500 mt-1">{roles.length} total</p>
          </div>
          <div className="flex-1 overflow-y-auto">
            {roles.map((role) => (
              <button
                key={role.id}
                onClick={() => setSelectedRoleId(role.id)}
                className={`w-full px-4 py-3 text-left border-b border-gray-100 hover:bg-gray-50 transition-colors ${
                  selectedRoleId === role.id
                    ? "bg-blue-50 border-l-4 border-l-blue-600"
                    : ""
                }`}
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <ShieldCheckIcon
                      className={`w-4 h-4 ${
                        selectedRoleId === role.id
                          ? "text-blue-600"
                          : "text-gray-400"
                      }`}
                    />
                    <span className="font-medium text-gray-900 text-sm">
                      {role.name}
                    </span>
                  </div>
                  {isStandardRole(role) && (
                    <span className="px-1.5 py-0.5 text-xs bg-blue-100 text-blue-700 rounded">
                      STD
                    </span>
                  )}
                </div>
                {role.description && (
                  <p className="text-xs text-gray-500 mt-1 truncate">
                    {role.description}
                  </p>
                )}
              </button>
            ))}
          </div>
          <div className="p-3 border-t border-gray-200">
            <button
              disabled={rolesLocked}
              className="w-full flex items-center justify-center gap-2 px-3 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors disabled:bg-gray-300"
            >
              <PlusIcon className="w-4 h-4" />
              New Role
            </button>
          </div>
        </div>
      </div>

      {/* Right Panel - Role Details & Permissions */}
      <div className="flex-1 bg-white rounded-lg border border-gray-200 overflow-y-auto">
        {currentRole && (
          <>
            <div className="px-6 py-4 border-b border-gray-200">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-3">
                    <h2 className="text-xl font-semibold text-gray-900">
                      {currentRole.name}
                    </h2>
                    {isStandardRole(currentRole) && (
                      <span className="px-2 py-1 text-xs font-medium bg-blue-100 text-blue-700 rounded">
                        Standard Role
                      </span>
                    )}
                  </div>
                  {currentRole.description && (
                    <p className="text-sm text-gray-600 mt-1">
                      {currentRole.description}
                    </p>
                  )}
                  <p className="text-sm text-gray-500 mt-2">
                    Last updated:{" "}
                    {new Date(currentRole.lastUpdatedAt).toLocaleDateString()}
                  </p>
                </div>
                <div className="flex gap-2">
                  <button
                    disabled={rolesLocked && !isStandardRole(currentRole)}
                    className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors disabled:opacity-50"
                  >
                    <PencilIcon className="w-4 h-4" />
                  </button>
                  {!isStandardRole(currentRole) && (
                    <button
                      disabled={rolesLocked}
                      className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors disabled:opacity-50"
                    >
                      <TrashIcon className="w-4 h-4" />
                    </button>
                  )}
                </div>
              </div>
            </div>

            <div className="p-6">
              <h3 className="text-sm font-semibold text-gray-900 mb-4">
                Permissions
              </h3>
              {permissionCategories.length === 0 ? (
                <p className="text-sm text-gray-500">
                  No permissions available.
                </p>
              ) : (
                <div className="space-y-4">
                  {permissionCategories.map((category) => (
                    <div
                      key={category.id}
                      className="border border-gray-200 rounded-lg p-4"
                    >
                      <div className="font-medium text-gray-900 mb-3">
                        {category.label}
                      </div>
                      <div className="grid grid-cols-2 gap-3">
                        {category.permissions.map((perm) => {
                          const hasPermission = roleHasPermission(
                            currentRole.id,
                            perm.id
                          );
                          return (
                            <label
                              key={perm.id}
                              className="flex items-center gap-2 cursor-pointer"
                              title={perm.description || perm.name}
                            >
                              <input
                                type="checkbox"
                                checked={hasPermission}
                                disabled={rolesLocked}
                                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 disabled:opacity-50"
                              />
                              <span className="text-sm text-gray-700">
                                {perm.name}
                              </span>
                            </label>
                          );
                        })}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );

  // Layout 2: Permission Matrix View
  const MatrixViewLayout = () => (
    <div className="space-y-6">
      <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider sticky left-0 bg-gray-50">
                  Role
                </th>
                {permissionCategories.map((category) => (
                  <th
                    key={category.id}
                    colSpan={category.permissions.length}
                    className="px-3 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-l border-gray-200"
                  >
                    {category.label}
                  </th>
                ))}
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="px-6 py-2 sticky left-0 bg-gray-50"></th>
                {permissionCategories.map((category) => (
                  <React.Fragment key={category.id}>
                    {category.permissions.map((perm, idx) => (
                      <th
                        key={perm.id}
                        className={`px-2 py-2 text-xs text-gray-500 ${
                          idx === 0 ? "border-l border-gray-200" : ""
                        }`}
                        title={perm.name}
                      >
                        {perm.action.charAt(0).toUpperCase()}
                      </th>
                    ))}
                  </React.Fragment>
                ))}
                <th className="px-6 py-2"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {roles.map((role) => (
                <tr key={role.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap sticky left-0 bg-white">
                    <div className="flex items-center gap-2">
                      <ShieldCheckIcon className="w-4 h-4 text-blue-600" />
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="text-sm font-medium text-gray-900">
                            {role.name}
                          </span>
                          {isStandardRole(role) && (
                            <span className="px-1.5 py-0.5 text-xs bg-blue-100 text-blue-700 rounded">
                              STD
                            </span>
                          )}
                        </div>
                        {role.description && (
                          <span className="text-xs text-gray-500">
                            {role.description}
                          </span>
                        )}
                      </div>
                    </div>
                  </td>
                  {permissionCategories.map((category) => (
                    <React.Fragment key={category.id}>
                      {category.permissions.map((perm, idx) => {
                        const hasPermission = roleHasPermission(
                          role.id,
                          perm.id
                        );
                        return (
                          <td
                            key={perm.id}
                            className={`px-2 py-4 text-center ${
                              idx === 0 ? "border-l border-gray-200" : ""
                            }`}
                          >
                            {hasPermission ? (
                              <CheckIcon className="w-4 h-4 text-green-600 mx-auto" />
                            ) : (
                              <XMarkIcon className="w-4 h-4 text-gray-300 mx-auto" />
                            )}
                          </td>
                        );
                      })}
                    </React.Fragment>
                  ))}
                  <td className="px-6 py-4 text-right whitespace-nowrap">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        disabled={rolesLocked && !isStandardRole(role)}
                        className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded disabled:opacity-50"
                      >
                        <PencilIcon className="w-4 h-4" />
                      </button>
                      {!isStandardRole(role) && (
                        <button
                          disabled={rolesLocked}
                          className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded disabled:opacity-50"
                        >
                          <TrashIcon className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <h1 className="text-2xl font-semibold text-gray-900">
            Roles & Permissions
          </h1>
          <div className="flex gap-3">
            <button
              onClick={() => setRolesLocked(!rolesLocked)}
              className={`flex items-center gap-2 px-4 py-2 rounded-lg border transition-colors ${
                rolesLocked
                  ? "border-red-300 bg-red-50 text-red-700 hover:bg-red-100"
                  : "border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
              }`}
            >
              {rolesLocked ? (
                <LockClosedIcon className="w-4 h-4" />
              ) : (
                <LockOpenIcon className="w-4 h-4" />
              )}
              {rolesLocked ? "Roles Locked" : "Lock Roles"}
            </button>
          </div>
        </div>
        <p className="text-gray-600">
          Define and manage organization-level roles and permissions. These
          settings will propagate to all projects.
        </p>
      </div>

      {/* Lock Warning Banner */}
      {rolesLocked && (
        <div className="mb-6 p-4 bg-amber-50 border border-amber-200 rounded-lg flex items-start gap-3">
          <ExclamationCircleIcon className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-amber-900">
              Roles & Permissions Locked
            </p>
            <p className="text-sm text-amber-700 mt-1">
              Projects cannot create or modify roles at the project level.
            </p>
          </div>
        </div>
      )}

      {/* Layout Selector */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          View Layout:
        </label>
        <div className="flex gap-2">
          <button
            onClick={() => setActiveLayout("split-view")}
            className={`px-4 py-2 text-sm rounded-lg transition-colors ${
              activeLayout === "split-view"
                ? "bg-blue-600 text-white"
                : "bg-white text-gray-700 border border-gray-300 hover:bg-gray-50"
            }`}
          >
            Split View
          </button>
          <button
            onClick={() => setActiveLayout("matrix")}
            className={`px-4 py-2 text-sm rounded-lg transition-colors ${
              activeLayout === "matrix"
                ? "bg-blue-600 text-white"
                : "bg-white text-gray-700 border border-gray-300 hover:bg-gray-50"
            }`}
          >
            Matrix View
          </button>
        </div>
      </div>

      {/* Render Selected Layout */}
      {activeLayout === "split-view" && <SplitViewLayout />}
      {activeLayout === "matrix" && <MatrixViewLayout />}
    </div>
  );
};

export default RolesAndPermissions;
