"use client";


import { ApiRecord, BulkMetadataPayload } from "@/app/(home)/types/bulk_upload_types";
import api from "./api";

/**
 * Uploads bulk metadata records to the API
 */
export async function uploadBulkMetadata(
  organizationId: number,
  projectId: number,
  dataSourceId: number,
  validRecords: ApiRecord[]
): Promise<any> {
  try {
    // Build the payload
    const payload: BulkMetadataPayload = {
      classes: [],
      relationships: [],
      tags: [],
      records: validRecords,
      edges: [],
    };

    // Use the api instance which handles auth headers
    const res = await api.post(
      `/organizations/${organizationId}/projects/${projectId}/datasources/${dataSourceId}/metadata`,
      payload
    );

    return res.data;
  } catch (error: any) {
    console.error("Error uploading bulk metadata:", error);
    
    // Re-throw the error with full response data so component can access it
    throw error;
  }
}