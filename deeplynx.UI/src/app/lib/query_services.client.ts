// src/app/lib/query_services.client.ts
"use client";

import axios from "axios";
import { CustomQueryRequestDto } from "../(home)/types/types";

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true,
});

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function queryBuilder(
    queryObj: CustomQueryRequestDto[],
    fullTextSearch: string | null
) {
    const res = await api.post(`/records/QueryBuilder?${fullTextSearch}`, queryObj, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
}
