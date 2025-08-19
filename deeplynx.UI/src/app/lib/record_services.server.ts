// src/app/lib/record_services.server.ts
import "server-only";

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

/** ===== Types (adapt to your app’s shared types if you have them) ===== */
export type RecordRow = {
  id: number;
  uri?: string | null;
  name?: string;
  properties?: string;
  originalId?: string;
  classId?: number;
  className?: string;
  mappingId?: string | null;
  dataSourceId?: number;
  dataSourceName?: string;
  projectId?: number;
  projectName?: string;
  tags: string;
  createdBy?: string | null;
  createdAt?: string | null;
  modifiedBy?: string | null;
  modifiedAt?: string | null;
  archivedAt?: string | null;
  lastUpdatedAt?: string;
  description?: string;
  fileType: string;
  timeseries?: boolean;
  fileSize?: number;
  select?: boolean;
  associatedRecords?: string[];
};

export type UpdateRecordPayload = {
  uri?: string | null;
  properties?: Record<string, unknown>;
  original_id?: string | null;
  name?: string | null;
  class_id?: number | null;
  class_name?: string | null;
  description?: string | null;
};

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
