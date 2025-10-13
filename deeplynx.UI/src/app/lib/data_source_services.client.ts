// src/app/lib/data_source_services.client.ts
"use client";

import api from "./api";

export type DataSourceDTO = {
  id: number;
  name: string;
  description?: string | null;
  is_archived?: boolean;
  lastUpdatedAt?: string | null;
  type?: string | null; 
};

/**
 * GET /projects/{projectId}/datasources/GetAllDataSources
 * Returns all non-archived data sources for a project.
 */
export async function getAllDataSources(projectId: number): Promise<DataSourceDTO[]> {
  try {
    const res = await api.get(
      `/projects/${projectId}/datasources/GetAllDataSources`,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data as DataSourceDTO[];
  } catch (error) {
    console.error("Error fetching data sources:", error);
    throw error;
  }
}
