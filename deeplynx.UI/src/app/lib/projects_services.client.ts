// src/app/lib/projects_services.client.ts
"use client";

import api from "./api";

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function getAllProjects() {
  const res = await api.get("/projects/GetAllProjects");
  return res.data;
}

export async function getAllRecordsForMultipleProjects(
  projectIds: number[] | string[],
  hideArchived = true,
  opts?: { signal?: AbortSignal }
) {
  const query =
    projectIds.map((id) => `projects=${encodeURIComponent(id)}`).join("&") +
    `&hideArchived=${hideArchived}`;

  return api.get(`/projects/MultiProjectRecords?${query}`, {
    signal: opts?.signal,
  }).then(r => r.data);
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

export async function addMember(
  projectId: string,
  userId?: number,
  groupId?: number,
  roleId?: number,
  name?: string,
  email?: string,
  role?: string,
  ) {
  try {
    console.log(`Adding member to project: ${userId} Name: ${name} and Role: ${role}`);
    const res = await api.post(`/projects/AddMemberToProject`, {
      params: { projectId, userId, role },
    });
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}