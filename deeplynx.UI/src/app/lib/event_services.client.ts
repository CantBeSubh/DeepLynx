// src/app/lib/event_services.client.ts
"use client"

import api from "./api"

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function getAllEvents(pageNumber: number = 1, pageSize: number = 25) {
    const res = await api.get("/events/GetAllEvents", {
        params: {
            pageNumber,
            pageSize
        }
    })
    return res.data
}

export async function getAllEventsByUser() {
    const res = await api.get("/events/GetAllEventsByUser")
    return res.data
}