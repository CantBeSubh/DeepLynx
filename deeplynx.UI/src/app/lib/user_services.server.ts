// src/app/lib/user_services.server.ts
import "server-only";
import { UserResponseDto } from "../(home)/types/responseDTOs";

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

export async function getAllUsersServer<T = UserResponseDto[]>(
  projectId: number
): Promise<T> {
  const qs = new URLSearchParams({ projectId: String(projectId) });
  const res = await fetch(`${BASE}/user/GetAllUsers?${qs.toString()}`, {
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<T>(res);
}

export async function getDataOverviewServer<T = unknown>(
  userId: string
): Promise<T> {
  const res = await fetch(`${BASE}/user/GetDataOverview/${encodeURIComponent(userId)}`, {
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
  const res = await fetch(`${BASE}/user/GetRecentlyAddedRecords?${qs.toString()}`, {
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<T>(res);
}

export async function updateUserServer<T = UserResponseDto>(
  userId: number,
  name?: string
): Promise<T> {
  const res = await fetch(`${BASE}/user/UpdateUser/${userId}`, {
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
  const res = await fetch(`${BASE}/user/DeleteUser/${userId}`, {
    method: 'DELETE',
    headers: authHeaders(),
  });
  return asJson<T>(res);
}
