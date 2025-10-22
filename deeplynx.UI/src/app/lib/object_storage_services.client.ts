// src/app/lib/object_storage_services.client.ts

import { ObjectStorageResponseDto } from "../(home)/types/responseDTOs";
import api from "./api";

export async function getAllObjectStorages(projectId: number | string) {
    if (!projectId) throw new Error("Project ID is required.");

    try {
        const res = await api.get<ObjectStorageResponseDto[]>(
            `/projects/${projectId}/storages/GetAllObjectStorages`
        );
        return res;
    } catch (error) {
        console.error("Error fetching object storages: ", error);
        throw error;
    }
}