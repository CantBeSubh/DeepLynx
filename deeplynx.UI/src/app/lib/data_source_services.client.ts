// src/app/lib/data_source_services.client.ts
"use client";

import api from "./api";
import { DataSourceResponseDto } from "../(home)/types/responseDTOs";

/**
 * GET /projects/{projectId}/datasources/GetAllDataSources
 * Returns all non-archived data sources for a project.
 */
export async function getAllDataSources(projectId: number): Promise<DataSourceResponseDto[]> {
  try {
    const res = await api.get(
      `/projects/${projectId}/datasources/GetAllDataSources`,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data as DataSourceResponseDto[];
  } catch (error) {
    console.error("Error fetching data sources:", error);
    throw error;
  }
}
