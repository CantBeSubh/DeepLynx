import "server-only";
import { SendEmailResponse } from "../(home)/types/types";
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

export async function sendEmailServer(email: string): Promise<SendEmailResponse> {
  const qs = new URLSearchParams({ email });
  const res = await fetch(`${BASE}/notification/SendEmail?${qs.toString()}`, {
    method: "POST",
    headers: authHeaders(),
    cache: "no-store",
  });
  return asJson<SendEmailResponse>(res);
}