// src/app/lib/permission_services.ts

import { CreatePermissionRequestDto, UpdatePermissionRequestDto } from "@/app/(home)/types/requestDTOs";
import { PermissionResponseDto } from "@/app/(home)/types/responseDTOs";
import api from "./api";


/**
 * Get all permissions for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param labelId - Optional sensitivity label ID to filter permissions
 * @param hideArchived - Flag to hide archived permissions (default: true)
 * @returns Promise with array of PermissionResponseDto
 */
export async function getAllPermissions(
  organizationId: number,
  projectId: number,
  labelId?: number,
  hideArchived: boolean = true
): Promise<PermissionResponseDto[]> {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/permissions`,
      { params: { labelId, hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error("Error getting all permissions:", error);
    throw error;
  }
}

/**
 * Get a specific permission by ID
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param permissionId - The ID of the permission
 * @param hideArchived - Flag to hide archived permissions (default: true)
 * @returns Promise with PermissionResponseDto
 */
export async function getPermissionById(
  organizationId: number,
  projectId: number,
  permissionId: number,
  hideArchived: boolean = true
): Promise<PermissionResponseDto> {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/permissions/${permissionId}`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting permission ${permissionId}:`, error);
    throw error;
  }
}

/**
 * Create a new permission
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dto - The permission creation request DTO
 * @returns Promise with PermissionResponseDto
 */
export async function createPermission(
  organizationId: number,
  projectId: number,
  dto: CreatePermissionRequestDto
): Promise<PermissionResponseDto> {
  try {
    const res = await api.post(
      `/organizations/${organizationId}/projects/${projectId}/permissions`,
      dto,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error creating permission:", error);
    throw error;
  }
}

/**
 * Update a permission
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param permissionId - The ID of the permission to update
 * @param dto - The permission update request DTO
 * @returns Promise with PermissionResponseDto
 */
export async function updatePermission(
  organizationId: number,
  projectId: number,
  permissionId: number,
  dto: UpdatePermissionRequestDto
): Promise<PermissionResponseDto> {
  try {
    const res = await api.put(
      `/organizations/${organizationId}/projects/${projectId}/permissions/${permissionId}`,
      dto,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error updating permission ${permissionId}:`, error);
    throw error;
  }
}

/**
 * Delete a permission
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param permissionId - The ID of the permission to delete
 * @returns Promise with success message
 */
export async function deletePermission(
  organizationId: number,
  projectId: number,
  permissionId: number
): Promise<{ message: string }> {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/projects/${projectId}/permissions/${permissionId}`
    );
    return res.data;
  } catch (error) {
    console.error(`Error deleting permission ${permissionId}:`, error);
    throw error;
  }
}

/**
 * Archive or unarchive a permission
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param permissionId - The ID of the permission to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export async function archivePermission(
  organizationId: number,
  projectId: number,
  permissionId: number,
  archive: boolean
): Promise<{ message: string }> {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/projects/${projectId}/permissions/${permissionId}`,
      null,
      { params: { archive } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error ${archive ? 'archiving' : 'unarchiving'} permission ${permissionId}:`, error);
    throw error;
  }
}