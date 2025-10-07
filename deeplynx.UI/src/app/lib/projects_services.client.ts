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

export async function updateProjectMemberRole(
  projectId: number,
  roleId: number,
  userId?: number,
  groupId?: number
) {
  const params = new URLSearchParams({
    projectId: projectId.toString(),
    roleId: roleId.toString(),
  });

  if (userId !== undefined) {
    params.append('userId', userId.toString());
  }
  if (groupId !== undefined) {
    params.append('groupId', groupId.toString());
  }

  const res = await api.put(`/projects/UpdateProjectMemberRole?${params.toString()}`, {
    headers: { "Content-Type": "application/json" },
  });
  return res.data;
}
export async function removeProjectMemberRole(
  projectId: number,
  userId?: number,
  groupId?: number
) {
  const queryParams = [
    `projectId=${projectId}`,
    userId !== undefined ? `userId=${userId}` : null,
    groupId !== undefined ? `groupId=${groupId}` : null,
  ]
    .filter(Boolean)
    .join('&');

  const res = await api.delete(
    `/projects/RemoveMemberFromProject?${queryParams}`,
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return res.data;
}

export async function getProjectMembers(projectId: number) {
  const res = await api.get(`/projects/GetProjectMembers/${projectId}`);
  return res.data;
}

//Function for when roles are not optional
// export async function addMember(
//   projectId: number,
//   userId: number,
//   roleId?: number,
//   groupId?: number,
// ) {
//   try {
//     console.log(`Adding member to project: ${userId}, Role: ${roleId}, Project: ${projectId}`);
//     const res = await api.post(`/projects/AddMemberToProject?projectId=${projectId}&roleId=${roleId}&userId=${userId}`, {
//       projectId,
//       userId,
//       roleId,
//       groupId,
//     });
//     return res.data;
//   } catch (error) {
//     console.error("API call failed:", error);
//     throw error;
//   }
// }

export async function addMember(
  projectId: number,
  userId: number,
  roleId?: number,
  groupId?: number,
) {
  try {
    const url = `/projects/AddMemberToProject?projectId=${projectId}&userId=${userId}` +
                (roleId !== undefined ? `&roleId=${roleId}` : '');

    const res = await api.post(url, { projectId, userId, roleId, groupId });
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}