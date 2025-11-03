// src/app/lib/graph_services.client.ts
"use client";

import { GraphResponseDto } from "../(home)/types/responseDTOs";
import api from "./api";

export interface GraphQueryParams {
  recordId?: number;
  depth?: number;
}

export async function getGraphDataForRecord(
  projectId: string,
  params?: GraphQueryParams
): Promise<GraphResponseDto> {
  try {
    const res = await api.get(
      `/projects/${projectId}/edges/GetGraphDataForRecord`,
      {
        params: {
          recordId: params?.recordId,
          depth: params?.depth,
        },
        headers: { "Content-Type": "application/json" },
      }
    );
    return res.data as GraphResponseDto;
  } catch (error) {
    console.error("Error fetching graph data:", error);
    throw error;
  }
}