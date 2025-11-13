import api from './api';
import { PermissionResponseDto, RoleResponseDto } from '../(home)/types/responseDTOs';

// ===== Role CRUD Operations =====

export async function getAllRoles(params?: {
  projectId?: number;
  organizationId?: number;
  hideArchived?: boolean;
}): Promise<RoleResponseDto[]> {
  try {
    const searchParams = new URLSearchParams();
    
    if (params?.projectId !== undefined) {
      searchParams.append("projectId", params.projectId.toString());
    }
    if (params?.organizationId !== undefined) {
      searchParams.append("organizationId", params.organizationId.toString());
    }
    if (params?.hideArchived !== undefined) {
      searchParams.append("hideArchived", params.hideArchived.toString());
    }

    const queryString = searchParams.toString();
    const url = `/roles/GetAllRoles${queryString ? `?${queryString}` : ""}`;
    
    const res = await api.get(url, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error getting all roles:", error);
    throw error;
  }
}

export async function getRoleById(
  roleId: number,
  params?: { hideArchived?: boolean }
): Promise<RoleResponseDto> {
  try {
    const searchParams = new URLSearchParams();
    
    if (params?.hideArchived !== undefined) {
      searchParams.append("hideArchived", params.hideArchived.toString());
    }

    const queryString = searchParams.toString();
    const url = `/roles/GetRole/${roleId}${queryString ? `?${queryString}` : ""}`;
    
    const res = await api.get(url, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error getting role by ID:", error);
    throw error;
  }
}

export async function createRole(
  body: {
    name: string;
    description?: string | null;
    projectId?: number | null;
    organizationId?: number | null;
  },
  params?: {
    projectId?: number;
    organizationId?: number;
  }
): Promise<RoleResponseDto> {
  try {
    const searchParams = new URLSearchParams();
    
    if (params?.projectId !== undefined) {
      searchParams.append("projectId", params.projectId.toString());
    }
    if (params?.organizationId !== undefined) {
      searchParams.append("organizationId", params.organizationId.toString());
    }

    const queryString = searchParams.toString();
    const url = `/roles/CreateRole${queryString ? `?${queryString}` : ""}`;
    
    const res = await api.post(url, body, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error creating role:", error);
    throw error;
  }
}

export async function updateRole(
  roleId: number,
  body: {
    name?: string | null;
    description?: string | null;
  }
): Promise<RoleResponseDto> {
  try {
    const res = await api.put(`/roles/UpdateRole/${roleId}`, body, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error updating role:", error);
    throw error;
  }
}

export async function deleteRole(roleId: number): Promise<void> {
  try {
    await api.delete(`/roles/DeleteRole/${roleId}`, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error deleting role:", error);
    throw error;
  }
}

export async function archiveRole(roleId: number): Promise<void> {
  try {
    await api.delete(`/roles/ArchiveRole/${roleId}`, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error archiving role:", error);
    throw error;
  }
}

export async function unarchiveRole(roleId: number): Promise<void> {
  try {
    await api.put(`/roles/UnarchiveRole/${roleId}`, {}, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error unarchiving role:", error);
    throw error;
  }
}

// ===== Permission Operations =====

export async function getPermissionsByRole(
  roleId: number
): Promise<PermissionResponseDto[]> {
  try {
    const res = await api.get(`/roles/GetPermissionsByRole/${roleId}`, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error getting permissions by role:", error);
    throw error;
  }
}

export async function addPermissionToRole(params: {
  roleId: number;
  permissionId: number;
}): Promise<void> {
  try {
    const searchParams = new URLSearchParams();
    searchParams.append("roleId", params.roleId.toString());
    searchParams.append("permissionId", params.permissionId.toString());

    await api.post(`/roles/AddPermissionToRole?${searchParams.toString()}`, {}, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error adding permission to role:", error);
    throw error;
  }
}

export async function removePermissionFromRole(params: {
  roleId: number;
  permissionId: number;
}): Promise<void> {
  try {
    const searchParams = new URLSearchParams();
    searchParams.append("roleId", params.roleId.toString());
    searchParams.append("permissionId", params.permissionId.toString());

    await api.delete(`/roles/RemovePermissionFromRole?${searchParams.toString()}`, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error removing permission from role:", error);
    throw error;
  }
}

export async function setPermissionsForRole(
  roleId: number,
  permissionIds: number[]
): Promise<void> {
  try {
    await api.put(`/roles/SetPermissionsForRole/${roleId}`, permissionIds, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error setting permissions for role:", error);
    throw error;
  }
}