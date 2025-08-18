// src/app/lib/projects_services.server.ts
import "server-only";
import type { FileViewerTableRow } from "@/app/(home)/types/types";

const BASE = process.env.BACKEND_BASE_URL || "https://nexus.dev.inl.gov/api";
const SERVICE_TOKEN = process.env.SERVICE_TOKEN || "xxx";

console.log("BACKEND BASE URL:",BASE)
console.log("SERVICE TOKEN:",SERVICE_TOKEN)

/** ----- Types ----- */
export type ProjectDTO = {
  id: number | string;
  name: string;
  description?: string | null;
  lastViewed?: string | null;
  createdAt?: string | null;
};

export type ProjectStatsDTO = Record<string, unknown>; // refine as you define your stats

/** Always return a HeadersInit (avoid union types) */
function authHeaders(): HeadersInit {
  const h: Record<string, string> = {};
  if (SERVICE_TOKEN) h.Authorization = `Bearer ${SERVICE_TOKEN}`;
  return h;
}

async function asJson<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`);
  return (await res.json()) as T;
}

/** ===== Server-safe calls (no cookies; safe for prerender/SSR) ===== */

export async function getAllProjectsServer(): Promise<ProjectDTO[]> {
  
  const res = await fetch(`${BASE}/projects/GetAllProjects`, {
    headers: authHeaders(),
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
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<FileViewerTableRow[]>(res);
}

export async function getProjectServer(
  projectId: string | number
): Promise<ProjectDTO> {
  const res = await fetch(`${BASE}/projects/GetProject/${projectId}`, {
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<ProjectDTO>(res);
}

export async function getProjectStatsServer(
  projectId: string | number
): Promise<ProjectStatsDTO> {
  const res = await fetch(`${BASE}/projects/ProjectStats/${projectId}`, {
    headers: authHeaders(),
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
    headers: { ...authHeaders(), "Content-Type": "application/json" },
    body: JSON.stringify(data),
    cache: "no-store",
  });
  return asJson<ProjectDTO>(res);
}
