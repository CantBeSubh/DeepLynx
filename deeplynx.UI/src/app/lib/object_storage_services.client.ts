// src/app/lib/object_storage_services.client.ts

import api from "./api";

export type ObjectStorageDTO = {
    id: number | string;
    name: string;
    type: string;
    projectId: number | string;
    default: boolean;
    lastUpdatedAt: string;
    lastUpdatedBy: string;
    isArchived: boolean;
}

export async function getAllObjectStorages(projectId: number | string) {
    if (!projectId) throw new Error("Project ID is required.");

    try {
        const res = await api.get<ObjectStorageDTO[]>(
            `/projects/${projectId}/storages/GetAllObjectStorages`
        );
        return res;
    } catch (error) {
        console.error("Error fetching object storages: ", error);
        throw error;
    }
}