// src/app/lib/user_services.server.ts
import "server-only";
import { UserResponseDto } from "../(home)/types/responseDTOs";
import apiServer from "./api.server"

const BASE = process.env.BACKEND_BASE_URL!;
const SERVICE_TOKEN = process.env.SERVICE_TOKEN || "";

function authHeaders(): HeadersInit {
  const h: Record<string, string> = {};
  if (SERVICE_TOKEN) h.Authorization = `Bearer ${SERVICE_TOKEN}`;
  return h;
}
async function asJson<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`);
  return (await res.json()) as T;
}

/** ---- Server-safe calls (no browser cookies; safe in prerender/SSR) ---- */

export async function getAllUsersServer(projectId?: number): Promise<UserResponseDto[]> {
  const params: Record<string, string> = {};
  if (projectId !== undefined) {
    params.projectId = String(projectId);
  }
  const qs = new URLSearchParams(params);
  return apiServer.get<UserResponseDto[]>(`users/GetAllUsers?${qs.toString()}`);
}


export async function getDataOverviewServer<T = unknown>(
  userId: string
): Promise<T> {
  const res = await fetch(`${BASE}/users/GetDataOverview/${encodeURIComponent(userId)}`, {
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<T>(res);
}

export async function getRecentlyAddedRecordsServer<T = unknown[]>(
  projectIds: string[]
): Promise<T> {
  const qs = new URLSearchParams();
  projectIds.forEach((id) => qs.append("projectId", id));
  const res = await fetch(`${BASE}/users/GetRecentlyAddedRecords?${qs.toString()}`, {
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<T>(res);
}

export async function updateUserServer<T = UserResponseDto>(
  userId: number,
  name?: string
): Promise<T> {
  const res = await fetch(`${BASE}/users/UpdateUser/${userId}`, {
    method: 'PUT',
    headers: {
      ...authHeaders(),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ name }),
  });
  return asJson<T>(res);
}

export async function deleteUserServer<T = void>(userId: number): Promise<T> {
  const res = await fetch(`${BASE}/users/DeleteUser/${userId}`, {
    method: 'DELETE',
    headers: authHeaders(),
  });
  return asJson<T>(res);
}
