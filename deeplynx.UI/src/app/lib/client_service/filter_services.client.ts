// src/app/lib/filter_services.client.ts
"use client";

import api from "./api";

/** Keep the exact behavior you had before (cookies + same payload shape) */
export async function filterRecords(filter: string) {
  try {
    const res = await api.post("/records/Filter", filter, { // TODO FIX
      headers: { "Content-Type": "application/json" },
    });
    return res.data;
  } catch (error) {
    console.error("Error filtering records:", error);
    throw error;
  }
}

export async function queryRecords(query: string) {
  try {
    const res = await api.get("/records/Filter/", { // TODO FIX
      params: { userQuery: query },
    });
    return res.data;
  } catch (error) {
    console.error("Query records failed:", error);
    throw error;
  }
}
