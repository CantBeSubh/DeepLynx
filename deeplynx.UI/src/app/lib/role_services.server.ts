import "server-only";
import { apiFetch, asJson } from "./api.server";
import { PermissionResponseDto, RoleResponseDto } from "../(home)/types/responseDTOs";

/** ===== Server-safe calls ===== */

export async function getAllRolesServer(params?: {
  projectId?: number;
  organizationId?: number;
  hideArchived?: boolean;
}): Promise<RoleResponseDto[]> {
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
  const path = `/roles/GetAllRoles${queryString ? `?${queryString}` : ""}`;
  
  const res = await apiFetch(path);
  return asJson<RoleResponseDto[]>(res);
}

export async function getRoleByIdServer(
  roleId: number,
  params?: { hideArchived?: boolean }
): Promise<RoleResponseDto> {
  const searchParams = new URLSearchParams();
  
  if (params?.hideArchived !== undefined) {
    searchParams.append("hideArchived", params.hideArchived.toString());
  }

  const queryString = searchParams.toString();
  const path = `/roles/GetRole/${roleId}${queryString ? `?${queryString}` : ""}`;
  
  const res = await apiFetch(path);
  return asJson<RoleResponseDto>(res);
}

export async function createRoleServer(
  body: RoleResponseDto,
  params?: {
    projectId?: number;
    organizationId?: number;
  }
): Promise<RoleResponseDto> {
  const searchParams = new URLSearchParams();
  
  if (params?.projectId !== undefined) {
    searchParams.append("projectId", params.projectId.toString());
  }
  if (params?.organizationId !== undefined) {
    searchParams.append("organizationId", params.organizationId.toString());
  }

  const queryString = searchParams.toString();
  const path = `/roles/CreateRole${queryString ? `?${queryString}` : ""}`;
  
  const res = await apiFetch(path, {
    method: "POST",
    body: JSON.stringify(body),
  });
  
  return asJson<RoleResponseDto>(res);
}

export async function updateRoleServer(
  roleId: number,
  body: {
    name?: string | null;
    description?: string | null;
  }
): Promise<RoleResponseDto> {
  const path = `/roles/UpdateRole/${roleId}`;
  
  const res = await apiFetch(path, {
    method: "PUT",
    body: JSON.stringify(body),
  });
  
  return asJson<RoleResponseDto>(res);
}

export async function deleteRoleServer(roleId: number): Promise<void> {
  const path = `/roles/DeleteRole/${roleId}`;
  
  await apiFetch(path, {
    method: "DELETE",
  });
}

export async function archiveRoleServer(roleId: number): Promise<void> {
  const path = `/roles/ArchiveRole/${roleId}`;
  
  await apiFetch(path, {
    method: "DELETE",
  });
}

export async function unarchiveRoleServer(roleId: number): Promise<void> {
  const path = `/roles/UnarchiveRole/${roleId}`;
  
  await apiFetch(path, {
    method: "PUT",
  });
}

export async function getPermissionsByRoleServer(
  roleId: number
): Promise<PermissionResponseDto[]> {
  const path = `/roles/GetPermissionsByRole/${roleId}`;
  
  const res = await apiFetch(path);
  return asJson<PermissionResponseDto[]>(res);
}

export async function addPermissionToRoleServer(params: {
  roleId: number;
  permissionId: number;
}): Promise<void> {
  const searchParams = new URLSearchParams();
  searchParams.append("roleId", params.roleId.toString());
  searchParams.append("permissionId", params.permissionId.toString());

  const path = `/roles/AddPermissionToRole?${searchParams.toString()}`;
  
  await apiFetch(path, {
    method: "POST",
  });
}

export async function removePermissionFromRoleServer(params: {
  roleId: number;
  permissionId: number;
}): Promise<void> {
  const searchParams = new URLSearchParams();
  searchParams.append("roleId", params.roleId.toString());
  searchParams.append("permissionId", params.permissionId.toString());

  const path = `/roles/RemovePermissionFromRole?${searchParams.toString()}`;
  
  await apiFetch(path, {
    method: "DELETE",
  });
}

export async function setPermissionsForRoleServer(
  roleId: number,
  permissionIds: number[]
): Promise<void> {
  const path = `/roles/SetPermissionsForRole/${roleId}`;
  
  await apiFetch(path, {
    method: "PUT",
    body: JSON.stringify(permissionIds),
  });
}