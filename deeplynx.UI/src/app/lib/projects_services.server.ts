// src/app/lib/projects_services.server.ts
import { ProjectResponseDto } from "../(home)/types/responseDTOs";
import "server-only";
import { auth } from "../../../auth";
import type { FileViewerTableRow } from "@/app/(home)/types/types";
import { ProjectStatResponseDto } from "../(home)/types/responseDTOs";
/** ----- Strict env handling (lazy) ----- */
let _BASE: string | null = null;

function getBase(): string {
  if (_BASE) return _BASE;

  const v = process.env.BACKEND_BASE_URL;
  if (!v) throw new Error("[ENV] BACKEND_BASE_URL is not set");
  if (!/^https?:\/\//.test(v)) {
    throw new Error(`[ENV] BACKEND_BASE_URL must start with http(s):// (got "${v}")`);
  }
  _BASE = v.replace(/\/+$/, ""); // strip trailing slash
  return _BASE;
}


// Optional: use a machine/service token in SSR when the user token isn't available
const SERVICE_TOKEN = process.env.BACKEND_SERVICE_TOKEN ?? process.env.SERVICE_TOKEN ?? "";


/** ----- Session helpers ----- */
type SessionShapeA = { tokens?: { access_token?: unknown } };
type SessionShapeB = { accessToken?: unknown };
type MaybeSession = SessionShapeA & SessionShapeB;

/** Safely pull an access token from unknown session shapes */
function extractAccessToken(x: unknown): string | null {
  if (typeof x !== "object" || x === null) return null;

  // tokens.access_token path
  const maybeTokens = (x as SessionShapeA).tokens;
  if (typeof maybeTokens === "object" && maybeTokens !== null) {
    const at = (maybeTokens as { access_token?: unknown }).access_token;
    if (typeof at === "string") return at;
  }

  // accessToken path
  const at2 = (x as SessionShapeB).accessToken;
  if (typeof at2 === "string") return at2;

  return null;
}

/** Get a JWT: prefer user token; fall back to service token for SSR */
async function getBearer(): Promise<string | null> {
  const session: unknown = await auth().catch(() => null);
  const userToken = extractAccessToken(session);
  return userToken ?? (SERVICE_TOKEN || null);
}


async function authHeaders(): Promise<HeadersInit> {
  const token = await getBearer();
  return {
    Accept: "application/json",
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

/** Small fetch wrapper with detailed error logging */
async function apiFetch(path: string, init: RequestInit = {}) {
  const url = `${getBase()}${path.startsWith("/") ? "" : "/"}${path}`;
  const headers = {
    ...(await authHeaders()),
    ...(init.headers || {}),
  };

  const res = await fetch(url, {
    ...init,
    headers,
    cache: "no-store",
  });

  if (!res.ok) {
    const body = await res.text().catch(() => "<no response body>");
    console.error("[API ERROR]", {
      method: init.method || "GET",
      url,
      status: res.status,
      statusText: res.statusText,
      respHeaders: Object.fromEntries(res.headers.entries()),
      body: body.slice(0, 2000),
    });
    throw new Error(`API ${init.method || "GET"} ${path} -> ${res.status}`);
  }
  return res;
}


async function asJson<T>(res: Response): Promise<T> {
  return (await res.json()) as T;
}

/** ===== Server-safe calls ===== */

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

//Function for when roles are not optional
// export async function addMemberServer(data: {
//   projectId: number;
//   userId: number;
//   roleId?: number;
//   groupId?: number;
// }): Promise<ProjectResponseDto> {
//   const res = await apiFetch(`/projects/AddMemberToProject?projectId=${data.projectId}&userId=${data.userId}&roleId=${data.roleId}`, {
//     method: "POST",
//     body: JSON.stringify(data),
//   });
//   return asJson<ProjectResponseDto>(res);
// }

export async function addMemberServer(data: {
  projectId: number;
  userId: number;
  roleId?: number;
  groupId?: number;
}): Promise<ProjectResponseDto> {
  // Construct the base URL with required parameters
  const url = `/projects/AddMemberToProject?projectId=${data.projectId}&userId=${data.userId}` +
              (data.roleId !== undefined ? `&roleId=${data.roleId}` : '');

  const res = await apiFetch(url, {
    method: "POST",
    body: JSON.stringify(data),
  });
  return asJson<ProjectResponseDto>(res);
}
