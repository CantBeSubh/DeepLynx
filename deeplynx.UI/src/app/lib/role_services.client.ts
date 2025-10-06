"use client";

import api from "./api";

export async function getAllRoles(projectId: number) {
  if (!projectId) {
    throw new Error("Project ID must be provided");
  }

  const res = await api.get(`/roles/GetAllRoles?projectId=${projectId}`);
  return res.data;
}