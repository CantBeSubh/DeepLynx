// src/app/lib/user_services.server.ts
import "server-only";
import { UserResponseDto } from "../../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";


/** ---- Server-safe calls (no browser cookies; safe in prerender/SSR) ---- */

export async function getAllUsersServer(projectId?: number, organizationId?: number): Promise<UserResponseDto[]> {
  const params: Record<string, string> = {};
  if (projectId !== undefined) {
    params.projectId = String(projectId);
  }
  if (organizationId !== undefined) {
    params.organizationId = String(organizationId);
  }
  const qs = new URLSearchParams(params);
  const res = await apiFetch(`users?${qs.toString()}`);
  return asJson<UserResponseDto[]>(res);
}

export async function getLocalDevUserServer(): Promise<UserResponseDto> {
  const res = await apiFetch(`users/superuser`);
  return asJson<UserResponseDto>(res);
}

export async function getDataOverviewServer<T = unknown>(
  userId: string
): Promise<T> {
  const res = await apiFetch(`users/${encodeURIComponent(userId)}/data-overview`); // TODO FIX
  return asJson<T>(res);
}

export async function getRecentlyAddedRecordsServer<T = unknown[]>(
  projectIds: string[]
): Promise<T> {
  const qs = new URLSearchParams();
  projectIds.forEach((id) => qs.append("projectId", id));
  const res = await apiFetch(`users/recently-added-records?${qs.toString()}`); // TODO FIX
  return asJson<T>(res);
}

export async function updateUserServer<T = UserResponseDto>(
  userId: number,
  name?: string
): Promise<T> {
  const res = await apiFetch(`users/${userId}`, {
    method: 'PUT',
    body: JSON.stringify({ name }),
  });
  return asJson<T>(res);
}

export async function deleteUserServer<T = void>(userId: number): Promise<T> {
  const res = await apiFetch(`users/${userId}`, {
    method: 'DELETE',
  });
  return asJson<T>(res);
}