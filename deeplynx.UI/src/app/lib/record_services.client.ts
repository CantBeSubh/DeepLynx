// src/app/lib/record_services.client.ts
"use client";

import api from "./api";

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

export type CreateRecordPayload = {
  name: string;
  original_id: string;
  description: string;

  properties: Record<string, unknown>;

  class_id?: number | null;
  object_storage_id?: number | null;
  uri?: string | null;
  class_name?: string | null;
  tags?: string[] | null;
  sensitivity_labels?: string[] | null;
};

export async function createRecord(
  projectId: number,
  payload: CreateRecordPayload,
  opts?: { dataSourceId?: number }
) {
  try {
    const res = await api.post(
      `/projects/${projectId}/records/CreateRecord`,
      payload,
      {
        headers: { "Content-Type": "application/json" },
        params:
          opts?.dataSourceId !== undefined
            ? { dataSourceId: opts.dataSourceId }
            : undefined,
      }
    );
    return res.data;
  } catch (error) {
    console.error("Error creating record: ", error);
    throw error;
  }
}
