// src/app/lib/record_services.client.ts
"use client";

import axios from "axios";
import api from "./api";

// export const api = axios.create({
//   baseURL: process.env.NEXT_PUBLIC_API_URL, // e.g., http://localhost:5095/api
//   withCredentials: true,                    // send browser cookies/session
// });

export async function getAllRecords(projectId: string) {
  try {
    const res = await api.get(`/projects/${projectId}/records/GetAllRecords`);
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}

export async function getRecord(projectId: number, recordId: number) {
  try {
    const res = await api.get(
      `/projects/${projectId}/records/historical/GetHistoricalRecord/${recordId}`
    );
    return res.data;
  } catch (error) {
    console.error("Error fetching record:", error);
    throw error;
  }
}

export async function updateRecord(
  projectId: number,
  recordId: number,
  updateData: {
    uri?: string | null;
    properties?: Record<string, unknown>;
    original_id?: string | null;
    name?: string | null;
    class_id?: number | null;
    class_name?: string | null;
    description?: string | null;
  }
) {
  try {
    const res = await api.put(
      `/projects/${projectId}/records/UpdateRecord/${recordId}`,
      updateData,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error updating record:", error);
    throw error;
  }
}

export const attachTagToRecord = async (projectId: number, recordId: number, tagId: number) => {
    try {
        const res = await api.post(
            `/projects/${projectId}/records/AttachTag/${recordId}`, 
            null,
            { params: { tagId: tagId } }
        );
        return res.data;
    } catch (error) {
        console.error("Error attaching tag to record:", error);
        throw error;
    }
};

export const unAttachTagFromRecord = async (projectId: number, recordId: number, tagId: number) => {
    try {
        const res = await api.post(
            `/projects/${projectId}/records/UnattachTag/${recordId}`, 
            null,
            { params: { tagId: tagId } }
        );
        return res.data;
    } catch (error) {
        console.error("Error unattaching tag to record:", error);
        throw error;
    }
};