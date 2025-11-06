// src/app/lib/user_services.server.ts
import "server-only";
import { UserResponseDto } from "../(home)/types/responseDTOs";
import { auth } from "../../../auth";

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

/** ---- Server-safe calls (no browser cookies; safe in prerender/SSR) ---- */

export async function getAllUsersServer(projectId?: number): Promise<UserResponseDto[]> {
  const params: Record<string, string> = {};
  if (projectId !== undefined) {
    params.projectId = String(projectId);
  }
  const qs = new URLSearchParams(params);
  const res = await apiFetch(`users/GetAllUsers?${qs.toString()}`);
  return asJson<UserResponseDto[]>(res);
}

export async function getDataOverviewServer<T = unknown>(
  userId: string
): Promise<T> {
  const res = await apiFetch(`users/GetDataOverview/${encodeURIComponent(userId)}`);
  return asJson<T>(res);
}

export async function getRecentlyAddedRecordsServer<T = unknown[]>(
  projectIds: string[]
): Promise<T> {
  const qs = new URLSearchParams();
  projectIds.forEach((id) => qs.append("projectId", id));
  const res = await apiFetch(`users/GetRecentlyAddedRecords?${qs.toString()}`);
  return asJson<T>(res);
}

export async function updateUserServer<T = UserResponseDto>(
  userId: number,
  name?: string
): Promise<T> {
  const res = await apiFetch(`users/UpdateUser/${userId}`, {
    method: 'PUT',
    body: JSON.stringify({ name }),
  });
  return asJson<T>(res);
}

export async function deleteUserServer<T = void>(userId: number): Promise<T> {
  const res = await apiFetch(`users/DeleteUser/${userId}`, {
    method: 'DELETE',
  });
  return asJson<T>(res);
}
