// src/app/lib/user_services.client.ts
"use client";

import api from "./api";

/** ---- Browser calls (with session cookies) ---- */

export async function getAllUsers(organizationId?: number | string, projectId?: number | string) {
  try {
    const params: Record<string, string | number | boolean> = {};
    
    if (organizationId !== undefined) {
      params.organizationId = organizationId;
    }
    
    if (projectId !== undefined) {
      params.projectId = projectId;
    }
    
    const res = await api.get(`/users/GetAllUsers`, { params });
    return res.data;
  } catch (error) {
    console.error("API call failed error getting all users:", error);
    throw error;
  }
}

export async function getDataOverview(userId: string) {
  try {
    const res = await api.get(`/users/GetDataOverview/${encodeURIComponent(userId)}`);
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
    const res = await api.get(`/users/GetRecentlyAddedRecords?${params.toString()}`);
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}

export async function updateUser(userId: number, name?: string) {
  try {
    const res = await api.put(`/users/UpdateUser/${userId}`, {
      name,
    });
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}

export async function deleteUser(userId: number) {
  try {
    const res = await api.delete(`/users/DeleteUser/${userId}`);
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}
