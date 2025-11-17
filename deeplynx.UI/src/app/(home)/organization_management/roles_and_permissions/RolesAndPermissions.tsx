// src/app/(home)/organization_management/roles_and_permissions/RolesAndPermissions.tsx

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  archiveRole,
  createRole,
  getPermissionsByRole,
  setPermissionsForRole,
  updateRole,
} from "@/app/lib/role_services.client";
import {
  CheckIcon,
  ExclamationCircleIcon,
  LockClosedIcon,
  LockOpenIcon,
  PencilIcon,
  PlusIcon,
  ShieldCheckIcon,
  TrashIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import React, { useEffect, useRef, useState } from "react";
import toast from "react-hot-toast";
import {
  PermissionResponseDto,
  RoleResponseDto,
} from "../../types/responseDTOs";
import CreateRoleModal from "./CreateRoleModal";
import DeleteRoleModal from "./DeleteRoleModal";
import EditRoleModal from "./EditRoleModal";

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
  const [isLoadingPermissions, setIsLoadingPermissions] = useState(false);
  const [initialLoadComplete, setInitialLoadComplete] = useState(false);

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const { organization } = useOrganizationSession();

  const handleCreateRole = async (data: {
    name: string;
    description: string | null;
  }) => {
    if (!organization?.organizationId) {
      throw new Error("No organization selected");
    }

    const orgId = Number(organization.organizationId);

    try {
      const newRole = await createRole(
        {
          name: data.name,
          description: data.description,
          organizationId: orgId,
        },
        { organizationId: orgId }
      );

      // Add the new role to the state
      setRoles((prev) => [...prev, newRole]);

      // Optionally select the new role
      setSelectedRoleId(newRole.id);
      toast.success("Created new role");
    } catch (error) {
      console.error("Error creating role:", error);
      toast.error("Failed to create new role");
      throw error;
    }
  };

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [roleToEdit, setRoleToEdit] = useState<RoleResponseDto | null>(null);

  const handleUpdateRole = async (
    roleId: number,
    data: { name?: string | null; description?: string | null }
  ) => {
    try {
      const updatedRole = await updateRole(roleId, data);

      setRoles((prev) =>
        prev.map((role) => (role.id === roleId ? updatedRole : role))
      );
      toast.success("Role updated successfully");
    } catch (error) {
      console.error("Error updating role:", error);
      toast.error("Failed to upload Role");
      throw error;
    }
  };

  const handleEditClick = (role: RoleResponseDto) => {
    setRoleToEdit(role);
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setIsEditModalOpen(false);
    setRoleToEdit(null);
  };

  // Add state (after your other modal states)
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [roleToDelete, setRoleToDelete] = useState<RoleResponseDto | null>(
    null
  );

  // Add delete handler
  const handleDeleteRole = async () => {
    if (!roleToDelete) return;

    try {
      await archiveRole(roleToDelete.id); // Changed from deleteRole to archiveRole

      // Remove the role from state (it's now archived, so hide it)
      setRoles((prev) => prev.filter((role) => role.id !== roleToDelete.id));

      // If the deleted role was selected, select another role or null
      if (selectedRoleId === roleToDelete.id) {
        const remainingRoles = roles.filter(
          (role) => role.id !== roleToDelete.id
        );
        setSelectedRoleId(remainingRoles[0]?.id || null);
      }

      toast.success("Role archived successfully");
    } catch (error) {
      console.error("Error archiving role:", error);
      toast.error("Failed to archive role");
      throw error;
    }
  };

  // Add handler to open delete modal
  const handleDeleteClick = (role: RoleResponseDto) => {
    setRoleToDelete(role);
    setIsDeleteModalOpen(true);
  };

  // Add handler to close delete modal
  const handleCloseDeleteModal = () => {
    setIsDeleteModalOpen(false);
    setRoleToDelete(null);
  };

  // Add state for permission editing (after your other state declarations)
  const [isEditingPermissions, setIsEditingPermissions] = useState(false);
  const [tempPermissions, setTempPermissions] = useState<Set<number>>(
    new Set()
  );

  // Add handler to start editing permissions
  const handleStartEditingPermissions = () => {
    if (!currentRole) return;

    // Initialize temp permissions with current permissions
    const currentPermissionIds =
      rolePermissions[currentRole.id]?.map((p) => Number(p.id)) || [];
    setTempPermissions(new Set(currentPermissionIds));
    setIsEditingPermissions(true);
  };

  // Add handler to toggle a permission in edit mode
  const handleTogglePermission = (permissionId: number) => {
    setTempPermissions((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(permissionId)) {
        newSet.delete(permissionId);
      } else {
        newSet.add(permissionId);
      }
      return newSet;
    });
  };

  // Add handler to save permission changes
  const handleSavePermissions = async () => {
    if (!currentRole) return;

    try {
      await setPermissionsForRole(currentRole.id, Array.from(tempPermissions));

      // Update the rolePermissions state
      const updatedPerms = permissions.filter((p) =>
        tempPermissions.has(Number(p.id))
      );
      setRolePermissions((prev) => ({
        ...prev,
        [currentRole.id]: updatedPerms,
      }));

      setIsEditingPermissions(false);
      toast.success("Permissions updated successfully");
    } catch (error) {
      console.error("Error updating permissions:", error);
      toast.error("Failed to update permissions");
      throw error;
    }
  };

  // Add handler to cancel editing
  const handleCancelEditingPermissions = () => {
    setIsEditingPermissions(false);
    setTempPermissions(new Set());
  };

  // Add this handler after your other handlers
  const handleRoleSelection = (roleId: number) => {
    if (isEditingPermissions) {
      toast.error("Please save or cancel your changes before switching roles");
      return;
    }
    setSelectedRoleId(roleId);
  };

  // Add state for matrix editing (after your other state declarations)
  const tableContainerRef = useRef<HTMLDivElement>(null);
  const [isEditingMatrix, setIsEditingMatrix] = useState(false);
  const [matrixTempPermissions, setMatrixTempPermissions] = useState<
    Record<number, Set<number>>
  >({});

  // Initialize matrix editing with current permissions for all roles
  const handleStartEditingMatrix = () => {
    const initialMatrix: Record<number, Set<number>> = {};

    roles.forEach((role) => {
      const rolePerms =
        rolePermissions[role.id]?.map((p) => Number(p.id)) || [];
      initialMatrix[role.id] = new Set(rolePerms);
    });

    setMatrixTempPermissions(initialMatrix);
    setIsEditingMatrix(true);
  };

  // Toggle a permission for a specific role in matrix edit mode
  const handleToggleMatrixPermission = (
    roleId: number,
    permissionId: number
  ) => {
    // Store current scroll position
    const scrollTop = tableContainerRef.current?.scrollTop || 0;
    const scrollLeft = tableContainerRef.current?.scrollLeft || 0;

    setMatrixTempPermissions((prev) => {
      const updated = { ...prev };
      if (!updated[roleId]) {
        updated[roleId] = new Set();
      }

      const roleSet = new Set(updated[roleId]);
      if (roleSet.has(permissionId)) {
        roleSet.delete(permissionId);
      } else {
        roleSet.add(permissionId);
      }

      updated[roleId] = roleSet;
      return updated;
    });

    // Restore scroll position after state update
    requestAnimationFrame(() => {
      if (tableContainerRef.current) {
        tableContainerRef.current.scrollTop = scrollTop;
        tableContainerRef.current.scrollLeft = scrollLeft;
      }
    });
  };

  // Save all matrix changes
  const handleSaveMatrixPermissions = async () => {
    try {
      // Update permissions for all roles that changed
      const updatePromises = roles.map((role) => {
        const newPermissions = Array.from(matrixTempPermissions[role.id] || []);
        return setPermissionsForRole(role.id, newPermissions);
      });

      await Promise.all(updatePromises);

      // Update the rolePermissions state for all roles
      const updatedRolePermissions: Record<number, PermissionResponseDto[]> =
        {};
      roles.forEach((role) => {
        const permIds = matrixTempPermissions[role.id] || new Set();
        updatedRolePermissions[role.id] = permissions.filter((p) =>
          permIds.has(Number(p.id))
        );
      });

      setRolePermissions((prev) => ({
        ...prev,
        ...updatedRolePermissions,
      }));

      setIsEditingMatrix(false);
      setMatrixTempPermissions({});
      toast.success("All permissions updated successfully");
    } catch (error) {
      console.error("Error updating matrix permissions:", error);
      toast.error("Failed to update permissions");
      throw error;
    }
  };

  // Cancel matrix editing
  const handleCancelEditingMatrix = () => {
    setIsEditingMatrix(false);
    setMatrixTempPermissions({});
  };

  // Check if a role has a permission in matrix edit mode
  const matrixRoleHasPermission = (
    roleId: number,
    permissionId: number
  ): boolean => {
    if (isEditingMatrix) {
      return matrixTempPermissions[roleId]?.has(permissionId) || false;
    }
    return roleHasPermission(roleId, permissionId);
  };

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

    setIsLoadingPermissions(true);
    try {
      const perms = await getPermissionsByRole(roleId);
      setRolePermissions((prev) => ({
        ...prev,
        [roleId]: perms,
      }));
    } catch (error) {
      console.error(`Error fetching permissions for role ${roleId}:`, error);
    } finally {
      setIsLoadingPermissions(false);
    }
  };

  // Add this function to fetch all permissions at once
  const fetchAllRolePermissions = async () => {
    setIsLoadingPermissions(true);
    try {
      const promises = roles.map((role) =>
        getPermissionsByRole(role.id)
          .then((perms) => ({ roleId: role.id, perms }))
          .catch((error) => {
            console.error(
              `Error fetching permissions for role ${role.id}:`,
              error
            );
            return { roleId: role.id, perms: [] };
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
    } finally {
      setIsLoadingPermissions(false);
    }
  };

  // Update the useEffect to fetch all permissions on mount
  useEffect(() => {
    if (!initialLoadComplete && roles.length > 0) {
      fetchAllRolePermissions();
    }
  }, [roles, initialLoadComplete]);

  // Keep the existing useEffect for individual role selection
  useEffect(() => {
    if (selectedRoleId && initialLoadComplete) {
      fetchRolePermissions(selectedRoleId);
    }
  }, [selectedRoleId, initialLoadComplete]);

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

  // Determine if role is "standard"
  const isStandardRole = (role: RoleResponseDto): boolean => {
    return (
      role.name === "Admin" || role.name === "User" || role.name === "Viewer"
    );
  };

  // Layout 1: Split View (Master-Detail)
  const SplitViewLayout = () => (
    <div className="flex gap-6" style={{ height: "calc(100vh - 27rem)" }}>
      {/* Left Sidebar - Roles List */}
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
                  disabled={isEditingPermissions}
                  className={`w-full px-4 py-3 text-left border-b border-base-300 transition-colors ${
                    selectedRoleId === role.id
                      ? "bg-primary/10 border-l-4 border-l-primary"
                      : ""
                  } ${
                    isEditingPermissions
                      ? "opacity-50 cursor-not-allowed"
                      : "hover:bg-base-200 cursor-pointer"
                  }`}
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
                  </div>
                  {role.description && (
                    <p className="text-xs text-base-content/60 mt-1 ml-6 truncate">
                      {role.description}
                    </p>
                  )}
                </button>
              ))}
            </div>
            <div className="p-3 border-t border-base-300">
              <button
                disabled={rolesLocked}
                onClick={() => setIsCreateModalOpen(true)}
                className="btn btn-primary btn-sm w-full gap-2"
              >
                <PlusIcon className="w-4 h-4" />
                New Role
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Right Panel - Role Details & Permissions */}
      <div className="flex-1 card bg-base-100 shadow-xl flex flex-col overflow-hidden border-2 border-primary">
        {currentRole ? (
          <>
            <div className="px-6 py-4 border-base-300 flex-shrink-0">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-3">
                    <h2 className="card-title">{currentRole.name}</h2>
                    {isStandardRole(currentRole) && (
                      <div className="badge badge-info">
                        Standard Role (Read-Only)
                      </div>
                    )}
                  </div>
                  {currentRole.description && (
                    <p className="text-sm text-base-content/70 mt-1">
                      {currentRole.description}
                    </p>
                  )}
                  <p className="text-sm text-base-content/60 mt-2">
                    Last updated:{" "}
                    {new Date(currentRole.lastUpdatedAt).toLocaleDateString()}
                  </p>
                </div>
                <div className="flex gap-2">
                  <button
                    disabled={isStandardRole(currentRole)} // Changed: always disabled for standard roles
                    onClick={() => handleEditClick(currentRole)}
                    className="btn btn-ghost btn-sm btn-circle"
                    title={
                      isStandardRole(currentRole)
                        ? "Standard roles cannot be edited"
                        : "Edit Role"
                    }
                  >
                    <PencilIcon className="size-6" />
                  </button>
                  {!isStandardRole(currentRole) && (
                    <button
                      disabled={rolesLocked}
                      onClick={() => handleDeleteClick(currentRole)}
                      className="btn btn-ghost btn-sm btn-circle text-error"
                      title="Delete Role"
                    >
                      <TrashIcon className="size-6" />
                    </button>
                  )}
                </div>
              </div>
            </div>

            <div className="divider px-3"></div>

            {/* Scrollable Permissions Section */}
            <div className="flex-1 overflow-y-auto p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold">Permissions</h3>
                {!isEditingPermissions ? (
                  <button
                    disabled={
                      rolesLocked ||
                      isStandardRole(currentRole) ||
                      isLoadingPermissions
                    }
                    onClick={handleStartEditingPermissions}
                    className="btn btn-primary btn-sm gap-2"
                    title={
                      isStandardRole(currentRole)
                        ? "Standard role permissions cannot be modified"
                        : rolesLocked
                        ? "Roles are locked"
                        : "Edit Permissions"
                    }
                  >
                    <PencilIcon className="w-4 h-4" />
                    Edit Permissions
                  </button>
                ) : (
                  <div className="flex gap-2">
                    <button
                      onClick={handleCancelEditingPermissions}
                      className="btn btn-ghost btn-sm"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleSavePermissions}
                      className="btn btn-primary btn-sm gap-2"
                    >
                      <CheckIcon className="w-4 h-4" />
                      Save Changes
                    </button>
                  </div>
                )}
              </div>

              {/* Add info alert for standard roles */}
              {isStandardRole(currentRole) && (
                <div className="alert alert-info mb-4">
                  <ExclamationCircleIcon className="w-5 h-5 flex-shrink-0" />
                  <span className="text-sm">
                    This is a standard role and cannot be modified. Create a
                    custom role if you need different permissions.
                  </span>
                </div>
              )}

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
                    <div key={category.id} className="card bg-base-200">
                      <div className="card-body p-4">
                        <h4 className="card-title text-sm mb-3">
                          {category.label}
                        </h4>
                        <div className="grid grid-cols-2 gap-3">
                          {category.permissions.map((perm) => {
                            const hasPermission = isEditingPermissions
                              ? tempPermissions.has(Number(perm.id))
                              : roleHasPermission(
                                  currentRole.id,
                                  Number(perm.id)
                                );

                            return (
                              <label
                                key={perm.id}
                                className={`label justify-start gap-2 ${
                                  isEditingPermissions
                                    ? "cursor-pointer"
                                    : "cursor-default"
                                }`}
                                title={perm.description || perm.name}
                              >
                                <input
                                  type="checkbox"
                                  checked={hasPermission}
                                  onChange={() =>
                                    handleTogglePermission(Number(perm.id))
                                  }
                                  disabled={!isEditingPermissions}
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

  // Layout 2: Permission Matrix View
  const MatrixViewLayout = () => (
    <div style={{ height: "calc(100vh - 27rem)" }}>
      <div className="card bg-base-100 shadow-xl h-full flex flex-col overflow-hidden">
        {/* Add Edit Controls Header */}
        <div className="px-6 py-3 border-b border-base-300 flex items-center justify-between">
          <h3 className="text-sm font-semibold">Permission Matrix</h3>
          {!isEditingMatrix ? (
            <button
              disabled={rolesLocked || isLoadingPermissions}
              onClick={handleStartEditingMatrix}
              className="btn btn-primary btn-sm gap-2"
            >
              <PencilIcon className="w-4 h-4" />
              Edit Matrix
            </button>
          ) : (
            <div className="flex gap-2">
              <button
                onClick={handleCancelEditingMatrix}
                className="btn btn-ghost btn-sm"
              >
                Cancel
              </button>
              <button
                onClick={handleSaveMatrixPermissions}
                className="btn btn-primary btn-sm gap-2"
              >
                <CheckIcon className="w-4 h-4" />
                Save All Changes
              </button>
            </div>
          )}
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
                          {/* Only show edit role button when not in matrix edit mode */}
                          {!isEditingMatrix && (
                            <button
                              disabled={isStandardRole(role)} // Changed: always disabled for standard roles
                              onClick={() => handleEditClick(role)}
                              className="btn btn-ghost btn-xs btn-circle"
                              title={
                                isStandardRole(role)
                                  ? "Standard roles cannot be edited"
                                  : "Edit Role"
                              }
                            >
                              <PencilIcon className="size-4" />
                            </button>
                          )}
                        </div>
                        {role.description && (
                          <span className="text-xs text-base-content/60 font-normal">
                            {role.description}
                          </span>
                        )}
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {permissionCategories.map((category) => (
                  <React.Fragment key={category.id}>
                    {/* Category Header Row */}
                    <tr className="bg-base-200">
                      <td
                        colSpan={roles.length + 2}
                        className="font-semibold text-sm sticky left-0"
                      >
                        {category.label}
                      </td>
                    </tr>
                    {/* Permission Rows */}
                    {category.permissions.map((perm) => (
                      <tr key={perm.id} className="hover">
                        <td className="sticky left-0 z-10">
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
                          const hasPermission = matrixRoleHasPermission(
                            role.id,
                            Number(perm.id)
                          );
                          const isStandard = isStandardRole(role); // Check if standard role

                          return (
                            <td key={role.id} className="text-center">
                              <div
                                onClick={(e) => {
                                  e.preventDefault();
                                  e.stopPropagation();
                                  // Only allow editing if in edit mode AND not a standard role
                                  if (isEditingMatrix && !isStandard) {
                                    handleToggleMatrixPermission(
                                      role.id,
                                      Number(perm.id)
                                    );
                                  }
                                }}
                                className={`inline-block ${
                                  isEditingMatrix && !isStandard
                                    ? "cursor-pointer hover:scale-110 transition-transform"
                                    : "cursor-default"
                                } ${
                                  isStandard && isEditingMatrix
                                    ? "opacity-50"
                                    : ""
                                }`} // Visually indicate standard roles are locked
                                title={
                                  isStandard && isEditingMatrix
                                    ? "Standard role permissions cannot be modified"
                                    : isEditingMatrix
                                    ? "Click to toggle"
                                    : hasPermission
                                    ? "Has permission"
                                    : "No permission"
                                }
                              >
                                {hasPermission ? (
                                  <CheckIcon
                                    className={`size-8 mx-auto ${
                                      isEditingMatrix && !isStandard
                                        ? "text-success hover:text-success/70"
                                        : "text-success"
                                    }`}
                                  />
                                ) : (
                                  <XMarkIcon
                                    className={`size-8 mx-auto ${
                                      isEditingMatrix && !isStandard
                                        ? "text-base-300 hover:text-success/50"
                                        : "text-base-300"
                                    }`}
                                  />
                                )}
                              </div>
                            </td>
                          );
                        })}
                        <td className="text-center">
                          {/* Removed individual permission edit button */}
                        </td>
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

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <h1 className="text-2xl font-bold">Roles & Permissions</h1>
          <button
            onClick={() => setRolesLocked(!rolesLocked)}
            className={`btn gap-2 ${rolesLocked ? "btn-error" : "btn-primary"}`}
          >
            {rolesLocked ? (
              <LockClosedIcon className="size-6" />
            ) : (
              <LockOpenIcon className="size-6" />
            )}
            {rolesLocked ? "Roles Locked" : "Lock Roles"}
          </button>
        </div>
        <p className="text-base-content/70">
          Define and manage organization-level roles and permissions. These
          settings will propagate to all projects.
        </p>
      </div>

      {/* Lock Warning Banner */}
      {rolesLocked && (
        <div className="alert alert-warning mb-6">
          <ExclamationCircleIcon className="w-5 h-5 flex-shrink-0" />
          <div>
            <h3 className="font-bold">Roles & Permissions Locked</h3>
            <div className="text-xs">
              Projects cannot create or modify roles at the project level.
            </div>
          </div>
        </div>
      )}

      {/* Layout Selector */}
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

      {/* Render Selected Layout */}
      {activeLayout === "split-view" && <SplitViewLayout />}
      {activeLayout === "matrix" && <MatrixViewLayout />}

      {/* Add the modals */}
      <CreateRoleModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleCreateRole}
        organizationId={organization?.organizationId || 0}
      />

      <EditRoleModal
        isOpen={isEditModalOpen}
        onClose={handleCloseEditModal}
        onSubmit={handleUpdateRole}
        role={roleToEdit}
      />

      <DeleteRoleModal
        isOpen={isDeleteModalOpen}
        onClose={handleCloseDeleteModal}
        onConfirm={handleDeleteRole}
        role={roleToDelete}
      />
    </div>
  );
};

export default RolesAndPermissions;
