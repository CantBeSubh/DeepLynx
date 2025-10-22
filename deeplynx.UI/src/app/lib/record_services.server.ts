// src/app/lib/record_services.server.ts
import "server-only";
import { RecordRow } from "../(home)/types/types";
import { UpdateRecordPayload } from "../(home)/types/types";
const BASE = process.env.BACKEND_BASE_URL!;
const SERVICE_TOKEN = process.env.SERVICE_TOKEN || "";

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

/** ===== Server-safe calls (no browser cookies; safe in build/prerender) ===== */

export async function getAllRecordsServer<T = RecordRow[]>(
  projectId: string | number
): Promise<T> {
  const res = await fetch(
    `${BASE}/projects/${projectId}/records/GetAllRecords`,
    {
      headers: authHeaders(),
      cache: "no-store",
    }
  );
  return asJson<T>(res);
}

export async function getRecordServer<T = RecordRow>(
  projectId: string | number,
  recordId: string | number
): Promise<T> {
  const res = await fetch(
    `${BASE}/projects/${projectId}/records/historical/GetHistoricalRecord/${recordId}`,
    {
      headers: authHeaders(),
      cache: "no-store",
    }
  );
  return asJson<T>(res);
}

export async function updateRecordServer<T = RecordRow>(
  projectId: string | number,
  recordId: string | number,
  updateData: UpdateRecordPayload
): Promise<T> {
  const res = await fetch(
    `${BASE}/projects/${projectId}/records/UpdateRecord/${recordId}`,
    {
      method: "PUT",
      headers: { ...authHeaders(), "Content-Type": "application/json" },
      body: JSON.stringify(updateData),
      cache: "no-store",
    }
  );
  return asJson<T>(res);
}
