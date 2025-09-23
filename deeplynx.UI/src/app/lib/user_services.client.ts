// src/app/lib/user_services.client.ts
"use client";

import api from "./api";

/** ---- Browser calls (with session cookies) ---- */

export async function getAllUsers(projectId: number) {
  try {
    const res = await api.get(`/user/GetAllUsers`, {
      params: { projectId },
    });
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}

export async function getDataOverview(userId: string) {
  try {
    const res = await api.get(`/user/GetDataOverview/${encodeURIComponent(userId)}`);
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}

export async function getRecentlyAddedRecords(projectIds: string[]) {
  try {
    const params = new URLSearchParams();
    projectIds.forEach((id) => params.append("projectId", id));
    const res = await api.get(`/user/GetRecentlyAddedRecords?${params.toString()}`);
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}
