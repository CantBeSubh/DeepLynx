// src/app/lib/filter_services.client.ts
"use client";

import axios from "axios";
import api from "./api";

// export const api = axios.create({
//   baseURL: process.env.NEXT_PUBLIC_API_URL,
//   withCredentials: true,
// });

/** Keep the exact behavior you had before (cookies + same payload shape) */
export async function filterRecords(filter: string) {
  try {
    const res = await api.post("/records/Filter", filter, {
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
    const res = await api.get("/records/Filter/", {
      params: { userQuery: query },
    });
    return res.data;
  } catch (error) {
    console.error("Query records failed:", error);
    throw error;
  }
}
