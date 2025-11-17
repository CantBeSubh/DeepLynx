// src/app/lib/permission_services.server.ts
import "server-only";
import { apiFetch, asJson } from "./api.server";
import { PermissionResponseDto } from "../(home)/types/responseDTOs";

/** ===== Server-safe calls ===== */

export async function getAllPermissionsServer(params?: {
  labelId?: number;
  projectId?: number;
  organizationId?: number;
  hideArchived?: boolean;
}): Promise<PermissionResponseDto[]> {
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
  const path = `/permissions/GetAllPermissions${queryString ? `?${queryString}` : ""}`;
  
  const res = await apiFetch(path);
  return asJson<PermissionResponseDto[]>(res);
}

export async function getPermissionByIdServer(
  permissionId: number,
  params?: { hideArchived?: boolean }
): Promise<PermissionResponseDto> {
  const searchParams = new URLSearchParams();
  
  if (params?.hideArchived !== undefined) {
    searchParams.append("hideArchived", params.hideArchived.toString());
  }

  const queryString = searchParams.toString();
  const path = `/permissions/GetPermission/${permissionId}${queryString ? `?${queryString}` : ""}`;
  
  const res = await apiFetch(path);
  return asJson<PermissionResponseDto>(res);
}

export async function createPermissionServer(
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
  const searchParams = new URLSearchParams();
  
  if (params?.projectId !== undefined) {
    searchParams.append("projectId", params.projectId.toString());
  }
  if (params?.organizationId !== undefined) {
    searchParams.append("organizationId", params.organizationId.toString());
  }

  const queryString = searchParams.toString();
  const path = `/permissions/CreatePermission${queryString ? `?${queryString}` : ""}`;
  
  const res = await apiFetch(path, {
    method: "POST",
    body: JSON.stringify(body),
  });
  
  return asJson<PermissionResponseDto>(res);
}

export async function deletePermissionServer(permissionId: number): Promise<void> {
  const path = `/permissions/DeletePermission/${permissionId}`;
  
  await apiFetch(path, {
    method: "DELETE",
  });
}

export async function archivePermissionServer(permissionId: number): Promise<void> {
  const path = `/permissions/ArchivePermission/${permissionId}`;
  
  await apiFetch(path, {
    method: "DELETE",
  });
}

export async function unarchivePermissionServer(permissionId: number): Promise<void> {
  const path = `/permissions/UnarchivePermission/${permissionId}`;
  
  await apiFetch(path, {
    method: "PUT",
  });
}