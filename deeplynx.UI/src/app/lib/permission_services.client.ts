// src/app/lib/permission_services.ts
import api from './api';
import { PermissionResponseDto } from '../(home)/types/responseDTOs';

// ===== Permission CRUD Operations =====

export async function getAllPermissions(params?: {
  labelId?: number;
  projectId?: number;
  organizationId?: number;
  hideArchived?: boolean;
}): Promise<PermissionResponseDto[]> {
  try {
    const searchParams = new URLSearchParams();
    
    if (params?.labelId !== undefined) {
      searchParams.append("labelId", params.labelId.toString());
    }
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
    const url = `/permissions/GetAllPermissions${queryString ? `?${queryString}` : ""}`;
    
    const res = await api.get(url, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error getting all permissions:", error);
    throw error;
  }
}

export async function getPermissionById(
  permissionId: number,
  params?: { hideArchived?: boolean }
): Promise<PermissionResponseDto> {
  try {
    const searchParams = new URLSearchParams();
    
    if (params?.hideArchived !== undefined) {
      searchParams.append("hideArchived", params.hideArchived.toString());
    }

    const queryString = searchParams.toString();
    const url = `/permissions/GetPermission/${permissionId}${queryString ? `?${queryString}` : ""}`;
    
    const res = await api.get(url, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error getting permission by ID:", error);
    throw error;
  }
}

export async function createPermission(
  body: {
    name: string;
    description?: string | null;
    action: string;
    labelId: number;
    projectId?: number | null;
    organizationId?: number | null;
  },
  params?: {
    projectId?: number;
    organizationId?: number;
  }
): Promise<PermissionResponseDto> {
  try {
    const searchParams = new URLSearchParams();
    
    if (params?.projectId !== undefined) {
      searchParams.append("projectId", params.projectId.toString());
    }
    if (params?.organizationId !== undefined) {
      searchParams.append("organizationId", params.organizationId.toString());
    }

    const queryString = searchParams.toString();
    const url = `/permissions/CreatePermission${queryString ? `?${queryString}` : ""}`;
    
    const res = await api.post(url, body, {
      headers: { "Content-Type": "application/json" }
    });

    return res.data;
  } catch (error) {
    console.error("Error creating permission:", error);
    throw error;
  }
}

export async function deletePermission(permissionId: number): Promise<void> {
  try {
    await api.delete(`/permissions/DeletePermission/${permissionId}`, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error deleting permission:", error);
    throw error;
  }
}

export async function archivePermission(permissionId: number): Promise<void> {
  try {
    await api.delete(`/permissions/ArchivePermission/${permissionId}`, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error archiving permission:", error);
    throw error;
  }
}

export async function unarchivePermission(permissionId: number): Promise<void> {
  try {
    await api.put(`/permissions/UnarchivePermission/${permissionId}`, {}, {
      headers: { "Content-Type": "application/json" }
    });
  } catch (error) {
    console.error("Error unarchiving permission:", error);
    throw error;
  }
}