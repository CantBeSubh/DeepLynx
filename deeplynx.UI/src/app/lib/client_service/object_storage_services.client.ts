import api from "./client_service/api";
import { ObjectStorageResponseDto } from "../(home)/types/responseDTOs";
import { CreateObjectStorageRequestDto, UpdateObjectStorageRequestDto } from "../(home)/types/requestDTOs";

/**
 * Get all object storages for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param hideArchived - Flag to hide archived object storages (default: true)
 * @returns Promise with array of ObjectStorageResponseDto
 */
export async function getAllObjectStorages(
    organizationId: number,
    projectId: number,
    hideArchived: boolean = true
): Promise<ObjectStorageResponseDto[]> {
    try {
        const res = await api.get<ObjectStorageResponseDto[]>(
            `/organizations/${organizationId}/projects/${projectId}/storages`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching object storages:", error);
        throw error;
    }
}

/**
 * Get a specific object storage by ID
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param objectStorageId - The ID of the object storage
 * @param hideArchived - Flag to hide archived object storages (default: true)
 * @returns Promise with ObjectStorageResponseDto
 */
export async function getObjectStorage(
    organizationId: number,
    projectId: number,
    objectStorageId: number,
    hideArchived: boolean = true
): Promise<ObjectStorageResponseDto> {
    try {
        const res = await api.get<ObjectStorageResponseDto>(
            `/organizations/${organizationId}/projects/${projectId}/storages/${objectStorageId}`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error fetching object storage ${objectStorageId}:`, error);
        throw error;
    }
}

/**
 * Get the default object storage for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @returns Promise with ObjectStorageResponseDto
 */
export async function getDefaultObjectStorage(
    organizationId: number,
    projectId: number
): Promise<ObjectStorageResponseDto> {
    try {
        const res = await api.get<ObjectStorageResponseDto>(
            `/organizations/${organizationId}/projects/${projectId}/storages/default`
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching default object storage:", error);
        throw error;
    }
}

/**
 * Create a new object storage
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dto - The object storage creation request DTO
 * @param makeDefault - Flag to make the created storage default (default: false)
 * @returns Promise with ObjectStorageResponseDto
 */
export async function createObjectStorage(
    organizationId: number,
    projectId: number,
    dto: CreateObjectStorageRequestDto,
    makeDefault: boolean = false
): Promise<ObjectStorageResponseDto> {
    try {
        const res = await api.post<ObjectStorageResponseDto>(
            `/organizations/${organizationId}/projects/${projectId}/storages`,
            dto,
            { params: { makeDefault } }
        );
        return res.data;
    } catch (error) {
        console.error("Error creating object storage:", error);
        throw error;
    }
}

/**
 * Update an object storage
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param objectStorageId - The ID of the object storage to update
 * @param dto - The object storage update request DTO
 * @returns Promise with ObjectStorageResponseDto
 */
export async function updateObjectStorage(
    organizationId: number,
    projectId: number,
    objectStorageId: number,
    dto: UpdateObjectStorageRequestDto
): Promise<ObjectStorageResponseDto> {
    try {
        const res = await api.put<ObjectStorageResponseDto>(
            `/organizations/${organizationId}/projects/${projectId}/storages/${objectStorageId}`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error(`Error updating object storage ${objectStorageId}:`, error);
        throw error;
    }
}

/**
 * Delete an object storage
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param objectStorageId - The ID of the object storage to delete
 * @returns Promise with success message
 */
export async function deleteObjectStorage(
    organizationId: number,
    projectId: number,
    objectStorageId: number
): Promise<{ message: string }> {
    try {
        const res = await api.delete<{ message: string }>(
            `/organizations/${organizationId}/projects/${projectId}/storages/${objectStorageId}`
        );
        return res.data;
    } catch (error) {
        console.error(`Error deleting object storage ${objectStorageId}:`, error);
        throw error;
    }
}

/**
 * Archive or unarchive an object storage
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param objectStorageId - The ID of the object storage to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export async function archiveObjectStorage(
    organizationId: number,
    projectId: number,
    objectStorageId: number,
    archive: boolean
): Promise<{ message: string }> {
    try {
        const res = await api.patch<{ message: string }>(
            `/organizations/${organizationId}/projects/${projectId}/storages/${objectStorageId}`,
            null,
            { params: { archive } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error ${archive ? 'archiving' : 'unarchiving'} object storage ${objectStorageId}:`, error);
        throw error;
    }
}

/**
 * Set an object storage as the default for the project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param objectStorageId - The ID of the object storage to set as default
 * @param isDefault - True to set as default (default: true)
 * @returns Promise with success message
 */
export async function setDefaultObjectStorage(
    organizationId: number,
    projectId: number,
    objectStorageId: number,
    isDefault: boolean = true
): Promise<{ message: string }> {
    try {
        const res = await api.patch<{ message: string }>(
            `/organizations/${organizationId}/projects/${projectId}/storages/${objectStorageId}/default`,
            null,
            { params: { isDefault } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error setting default object storage ${objectStorageId}:`, error);
        throw error;
    }
}