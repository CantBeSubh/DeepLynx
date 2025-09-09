// src/app/lib/query_services.client.ts
"use client";

import axios from "axios";
import { ClassResponseDto, CustomQueryRequestDto, DataSourceResponseDto, TagResponseDto } from "../(home)/types/types";

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true,
});

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function queryBuilder(
    queryObj: CustomQueryRequestDto[],
    fullTextSearch: string | null
) {
    //Building json string format from key/value input
    for (let obj of queryObj) {
        if (obj.jsonKey && obj.jsonValue) {
            let json = `{"${obj.jsonKey}": "${obj.jsonValue}"}`
            obj.json = json;
        }
    }
    const res = await api.post(`/records/QueryBuilder?${fullTextSearch}`, queryObj, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
}


export async function getClassesForProjects(
    projectIds: string[],
): Promise<ClassResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/records/GetAllClasses?${query}`);
    return res.data as ClassResponseDto[];
}

export async function getDataSourcesForProjects(
    projectIds: string[]
): Promise<DataSourceResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/records/GetAllDataSources?${query}`);
    return res.data as DataSourceResponseDto[];
}

export async function getTagsForProjects(
    projectIds: string[]
): Promise<TagResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/records/GetAllTags?${query}`);
    return res.data as TagResponseDto[];
}
