import { ObjectStorageResponseDto } from "../(home)/types/responseDTOs";
import { CreateObjectStorageRequestDto, UpdateObjectStorageRequestDto } from "../(home)/types/requestDTOs";
import api from "./api";

export async function getAllObjectStorages(
    projectId: number | string,
    hidearchived: boolean = true
) {
    if (!projectId) throw new Error("Project ID is required.");

    try {
        const res = await api.get<ObjectStorageResponseDto[]>(
            `/projects/${projectId}/storages/GetAllObjectStorages`,
            { params: { hidearchived } }
        );
        return res;
    } catch (error) {
        console.error("Error fetching object storages: ", error);
        throw error;
    }
}

export async function getObjectStorage(
    projectId: number | string,
    objectStorageId: number | string,
    hidearchived: boolean = true
) {
    if (!projectId) throw new Error("Project ID is required.");
    if (!objectStorageId) throw new Error("Object Storage ID is required.");

    try {
        const res = await api.get<ObjectStorageResponseDto>(
            `/projects/${projectId}/storages/GetObjectStorage/${objectStorageId}`,
            { params: { hidearchived } }
        );
        return res;
    } catch (error) {
        console.error(`Error fetching object storage ${objectStorageId}: `, error);
        throw error;
    }
}

export async function getDefaultObjectStorage(projectId: number | string) {
    if (!projectId) throw new Error("Project ID is required.");

    try {
        const res = await api.get<ObjectStorageResponseDto>(
            `/projects/${projectId}/storages/GetDefaultObjectStorage`
        );
        return res;
    } catch (error) {
        console.error("Error fetching default object storage: ", error);
        throw error;
    }
}

export async function createObjectStorage(
    projectId: number | string,
    dto: CreateObjectStorageRequestDto,
    makeDefault: boolean = false
) {
    if (!projectId) throw new Error("Project ID is required.");

    try {
        const res = await api.post<ObjectStorageResponseDto>(
            `/projects/${projectId}/storages/CreateObjectStorage`,
            dto,
            { params: { makeDefault } }
        );
        return res;
    } catch (error) {
        console.error("Error creating object storage: ", error);
        throw error;
    }
}

export async function updateObjectStorage(
    projectId: number | string,
    objectStorageId: number | string,
    dto: UpdateObjectStorageRequestDto
) {
    if (!projectId) throw new Error("Project ID is required.");
    if (!objectStorageId) throw new Error("Object Storage ID is required.");

    try {
        const res = await api.put<ObjectStorageResponseDto>(
            `/projects/${projectId}/storages/UpdateObjectStorage/${objectStorageId}`,
            dto
        );
        return res;
    } catch (error) {
        console.error(`Error updating object storage ${objectStorageId}: `, error);
        throw error;
    }
}

export async function deleteObjectStorage(
    projectId: number | string,
    objectStorageId: number | string
) {
    if (!projectId) throw new Error("Project ID is required.");
    if (!objectStorageId) throw new Error("Object Storage ID is required.");

    try {
        const res = await api.delete<{ message: string }>(
            `/projects/${projectId}/storages/DeleteObjectStorage/${objectStorageId}`
        );
        return res;
    } catch (error) {
        console.error(`Error deleting object storage ${objectStorageId}: `, error);
        throw error;
    }
}

export async function archiveObjectStorage(
    projectId: number | string,
    objectStorageId: number | string
) {
    if (!projectId) throw new Error("Project ID is required.");
    if (!objectStorageId) throw new Error("Object Storage ID is required.");

    try {
        const res = await api.delete<{ message: string }>(
            `/projects/${projectId}/storages/ArchiveObjectStorage/${objectStorageId}`
        );
        return res;
    } catch (error) {
        console.error(`Error archiving object storage ${objectStorageId}: `, error);
        throw error;
    }
}

export async function unarchiveObjectStorage(
    projectId: number | string,
    objectStorageId: number | string
) {
    if (!projectId) throw new Error("Project ID is required.");
    if (!objectStorageId) throw new Error("Object Storage ID is required.");

    try {
        const res = await api.put<{ message: string }>(
            `/projects/${projectId}/storages/UnarchiveObjectStorage/${objectStorageId}`,
            {}
        );
        return res;
    } catch (error) {
        console.error(`Error unarchiving object storage ${objectStorageId}: `, error);
        throw error;
    }
}

export async function setDefaultObjectStorage(
    projectId: number | string,
    objectStorageId: number | string
) {
    if (!projectId) throw new Error("Project ID is required.");
    if (!objectStorageId) throw new Error("Object Storage ID is required.");

    try {
        const res = await api.put<{ message: string }>(
            `/projects/${projectId}/storages/SetDefaultObjectStorage/${objectStorageId}`,
            {}
        );
        return res;
    } catch (error) {
        console.error(`Error setting default object storage ${objectStorageId}: `, error);
        throw error;
    }
}