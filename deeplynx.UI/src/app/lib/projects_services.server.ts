// src/app/lib/projects_services.server.ts
import "server-only";
import { ProjectResponseDto, ProjectStatResponseDto } from "../(home)/types/responseDTOs";
import type { FileViewerTableRow } from "@/app/(home)/types/types";
import { apiFetch, asJson } from "./api.server";

export async function getAllProjectsServer(): Promise<ProjectResponseDto[]> {
  const res = await apiFetch("/projects/GetAllProjects");
  return asJson<ProjectResponseDto[]>(res);
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

export async function getProjectServer(
  projectId: string | number
): Promise<ProjectResponseDto> {
  const res = await apiFetch(`/projects/GetProject/${projectId}`);
  return asJson<ProjectResponseDto>(res);
}

export async function getProjectStatsServer(
  projectId: string | number
): Promise<ProjectStatResponseDto> {
  const res = await apiFetch(`/projects/ProjectStats/${projectId}`);
  return asJson<ProjectStatResponseDto>(res);
}

export async function createProjectServer(data: {
  name: string;
  abbreviation: string | null;
  description: string | null;
}): Promise<ProjectResponseDto> {
  const res = await apiFetch(`/projects/CreateProject`, {
    method: "POST",
    body: JSON.stringify(data),
  });
  return asJson<ProjectResponseDto>(res);
}

export async function addMemberServer(data: {
  projectId: number;
  userId: number;
  roleId?: number;
  groupId?: number;
}): Promise<ProjectResponseDto> {
  const url = `/projects/AddMemberToProject?projectId=${data.projectId}&userId=${data.userId}` +
              (data.roleId !== undefined ? `&roleId=${data.roleId}` : '');

  const res = await apiFetch(url, {
    method: "POST",
    body: JSON.stringify(data),
  });
  return asJson<ProjectResponseDto>(res);
}