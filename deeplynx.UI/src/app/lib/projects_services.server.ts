// src/app/lib/projects_services.server.ts
import "server-only";
import { ProjectResponseDto, ProjectStatResponseDto } from "../(home)/types/responseDTOs";
import type { FileViewerTableRow } from "@/app/(home)/types/types";
import { apiFetch, asJson } from "./api.server";

export async function getAllProjectsServer(
  organizationId: number,
  hideArchived: boolean = true
): Promise<ProjectResponseDto[]> {
  const params = new URLSearchParams();
  params.append('hideArchived', String(hideArchived));

  const res = await apiFetch(
    `/organizations/${organizationId}/projects?${params.toString()}`
  );
  return asJson<ProjectResponseDto[]>(res);
}

export async function getProjectServer(
  organizationId: number,
  projectId: number,
  hideArchived: boolean = true
): Promise<ProjectResponseDto> {
  const params = new URLSearchParams();
  params.append('hideArchived', String(hideArchived));

  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}?${params.toString()}`
  );
  return asJson<ProjectResponseDto>(res);
}

export async function getProjectStatsServer(
  organizationId: number,
  projectId: number
): Promise<ProjectStatResponseDto> {
  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}/stats`
  );
  return asJson<ProjectStatResponseDto>(res);
}

export async function createProjectServer(
  organizationId: number,
  data: {
    name: string;
    abbreviation?: string | null;
    description?: string | null;
  }
): Promise<ProjectResponseDto> {
  const res = await apiFetch(
    `/organizations/${organizationId}/projects`,
    {
      method: "POST",
      body: JSON.stringify(data),
    }
  );
  return asJson<ProjectResponseDto>(res);
}

export async function updateProjectServer(
  organizationId: number,
  projectId: number,
  data: {
    organizationId: number;
    name?: string;
    abbreviation?: string | null;
    description?: string | null;
  }
): Promise<ProjectResponseDto> {
  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}`,
    {
      method: "PUT",
      body: JSON.stringify(data),
    }
  );
  return asJson<ProjectResponseDto>(res);
}

export async function deleteProjectServer(
  organizationId: number,
  projectId: number
): Promise<{ message: string }> {
  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}`,
    {
      method: "DELETE",
    }
  );
  return asJson<{ message: string }>(res);
}

export async function addMemberServer(
  organizationId: number,
  projectId: number,
  data: {
    roleId?: number;
    userId?: number;
    groupId?: number;
  }
): Promise<{ message: string }> {
  const params = new URLSearchParams();
  if (data.roleId !== undefined) params.append('roleId', String(data.roleId));
  if (data.userId !== undefined) params.append('userId', String(data.userId));
  if (data.groupId !== undefined) params.append('groupId', String(data.groupId));

  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}/members?${params.toString()}`,
    {
      method: "POST",
    }
  );
  return asJson<{ message: string }>(res);
}

export async function updateProjectMemberRoleServer(
  organizationId: number,
  projectId: number,
  data: {
    roleId: number;
    userId?: number;
    groupId?: number;
  }
): Promise<{ message: string }> {
  const params = new URLSearchParams();
  params.append('roleId', String(data.roleId));
  if (data.userId !== undefined) params.append('userId', String(data.userId));
  if (data.groupId !== undefined) params.append('groupId', String(data.groupId));

  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}/members?${params.toString()}`,
    {
      method: "PUT",
    }
  );
  return asJson<{ message: string }>(res);
}

export async function removeMemberFromProjectServer(
  organizationId: number,
  projectId: number,
  data: {
    userId?: number;
    groupId?: number;
  }
): Promise<{ message: string }> {
  const params = new URLSearchParams();
  if (data.userId !== undefined) params.append('userId', String(data.userId));
  if (data.groupId !== undefined) params.append('groupId', String(data.groupId));

  const res = await apiFetch(
    `/organizations/${organizationId}/projects/${projectId}/members?${params.toString()}`,
    {
      method: "DELETE",
    }
  );
  return asJson<{ message: string }>(res);
}

export async function getAllRecordsForMultipleProjectsServer(
  projectIds: number[],
  hideArchived = true
): Promise<FileViewerTableRow[]> {
  const query =
    projectIds.map((id) => `projects=${encodeURIComponent(id)}`).join("&") +
    `&hideArchived=${hideArchived}`;
  const res = await apiFetch(`/projects/MultiProjectRecords?${query}`);
  return asJson<FileViewerTableRow[]>(res);
}