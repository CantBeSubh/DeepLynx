// src/app/lib/query_services.client.ts
"use client";

import { CustomQueryRequestDto } from "../(home)/types/types";
import api from "./api";

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
