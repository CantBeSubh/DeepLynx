// src/app/lib/projects_services.server.ts
import "server-only";
import { auth } from "../../../auth";
import type { FileViewerTableRow } from "@/app/(home)/types/types";

const BASE = process.env.BACKEND_BASE_URL || "BASE URL IS NOT DEFINED";

/** ----- Types ----- */
export type ProjectDTO = {
  id: number | string;
  name: string;
  description?: string | null;
  lastViewed?: string | null;
  createdAt?: string | null;
};

export type ProjectStatsDTO = Record<string, unknown>; // refine as you define your stats

// /** Always return a HeadersInit (avoid union types) */
// function authHeaders(): HeadersInit {
//   const h: Record<string, string> = {};
//   if (SERVICE_TOKEN) h.Authorization = `Bearer ${SERVICE_TOKEN}`;
//   return h;
// }

// async function asJson<T>(res: Response): Promise<T> {
//   if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`);
//   return (await res.json()) as T;
// }

/** Get user JWT from server session */
async function getUserJWT(): Promise<string> {
  const session = await auth();
  const userJWT = (session as any)?.tokens?.access_token;
  
  if (!userJWT) {
    throw new Error("User not authenticated - no JWT available");
  }
  
  return userJWT;
}

/** Headers with user JWT */
async function userAuthHeaders(): Promise<HeadersInit> {
  const jwt = await getUserJWT();
  return {
    Authorization: `Bearer ${jwt}`,
    "Content-Type": "application/json"
  };
}

async function asJson<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`);
  return (await res.json()) as T;
}


/** ===== Server-safe calls (no cookies; safe for prerender/SSR) ===== */

export async function getAllProjectsServer(): Promise<ProjectDTO[]> {
  
  const res = await fetch(`${BASE}/projects/GetAllProjects`, {
    headers: await userAuthHeaders(),
    cache: "no-store", // or: next: { revalidate: 300 } for ISR
  });
  return asJson<ProjectDTO[]>(res);
}

export async function getAllRecordsForMultipleProjectsServer(
  projectIds: number[],
  hideArchived = true
): Promise<FileViewerTableRow[]> {
  const query =
    projectIds.map((id) => `projects=${encodeURIComponent(id)}`).join("&") +
    `&hideArchived=${hideArchived}`;

  const res = await fetch(`${BASE}/projects/MultiProjectRecords?${query}`, {
    headers: await userAuthHeaders(),
    cache: "no-store",
  });
  return asJson<FileViewerTableRow[]>(res);
}

export async function getProjectServer(
  projectId: string | number
): Promise<ProjectDTO> {
  const res = await fetch(`${BASE}/projects/GetProject/${projectId}`, {
    headers: await userAuthHeaders(),
    cache: "no-store",
  });
  return asJson<ProjectDTO>(res);
}

export async function getProjectStatsServer(
  projectId: string | number
): Promise<ProjectStatsDTO> {
  const res = await fetch(`${BASE}/projects/ProjectStats/${projectId}`, {
    headers: await userAuthHeaders(),
    cache: "no-store",
  });
  return asJson<ProjectStatsDTO>(res);
}

export async function createProjectServer(data: {
  name: string;
  abbreviation: string | null;
  description: string | null;
}): Promise<ProjectDTO> {
  const res = await fetch(`${BASE}/projects/CreateProject`, {
    method: "POST",
    headers: await userAuthHeaders(),
    body: JSON.stringify(data),
    cache: "no-store",
  });
  return asJson<ProjectDTO>(res);
}