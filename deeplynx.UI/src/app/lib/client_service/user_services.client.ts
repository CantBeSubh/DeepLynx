// src/app/lib/user_services.client.ts
"use client";

import { RecordResponseDto, UserResponseDto } from "@/app/(home)/types/responseDTOs";
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

    const res = await api.get(`/users`, { params });
    return res.data;
  } catch (error) {
    console.error("API call failed error getting all users:", error);
    throw error;
  }
}

export async function getCurrentUser() {
  try {
    const res = await api.get(`/users/current`);
    return res.data;
  } catch (error) {
    console.error("API call failed getting current user:", error);
    throw error;
  }
}

// user_services.client.ts (wherever your service is located)

export async function getLocalDevUser() {
  try {
    const res = await api.get(`/users/superuser`);
    return res.data;
  } catch (error) {
    console.error("API call failed getting local dev user:", error);
    throw error;
  }
}

export async function getDataOverview(userId: string) {
  try {
    const res = await api.get(`/users/${userId}/overview`); 
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}

export async function updateUser(
  userId: number,
  data: {
    name?: string | null;
    username?: string | null;
    isArchived?: boolean | null;
    projectId?: number | null;
    isActive?: boolean | null;
  }
): Promise<UserResponseDto> {
  try {
    const res = await api.put<UserResponseDto>(`/users/${userId}`, data);
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}
