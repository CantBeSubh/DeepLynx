// src/app/lib/projects_services.client.ts
"use client";

import axios from "axios";

/**
 * ENV (public):
 * NEXT_PUBLIC_API_URL: e.g. http://localhost:5095/api
 * - Browser requests will include cookies via withCredentials.
 */
export const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  withCredentials: true,
});

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function getAllProjects() {
  const res = await api.get("/projects/GetAllProjects");
  return res.data;
}

export async function getAllRecordsForMultipleProjects(
  projectIds: number[],
  hideArchived = true
) {
  const query =
    projectIds.map((id) => `projects=${encodeURIComponent(id)}`).join("&") +
    `&hideArchived=${hideArchived}`;
  const res = await api.get(`/projects/MultiProjectRecords?${query}`);
  return res.data;
}

export async function getProject(projectId: string) {
  const res = await api.get(`/projects/GetProject/${projectId}`);
  return res.data;
}

export async function getProjectStats(projectId: string) {
  const res = await api.get(`/projects/ProjectStats/${projectId}`);
  return res.data;
}

export async function createProject(data: {
  name: string;
  abbreviation: string | null;
  description: string | null;
}) {
  const res = await api.post("/projects/CreateProject", data, {
    headers: { "Content-Type": "application/json" },
  });
  return res.data;
}
