// src/app/lib/query_services.client.ts
"use client";

import { ClassResponseDto, CustomQueryRequestDto, DataSourceResponseDto, TagResponseDto } from "../(home)/types/types";
import api from "./api";

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function queryBuilder(
    queryObj: CustomQueryRequestDto[],
    fullTextSearch: string | null
) {
    //Building json string format from key/value input
    for (const obj of queryObj) {
        if (obj.jsonKey && obj.jsonValue) {
            const json = `{"${obj.jsonKey}": "${obj.jsonValue}"}`
            obj.json = json;
        }
    }

    const query = (`textSearch=${fullTextSearch}`);
    const res = await api.post(`/records/QueryBuilder?${query}`, queryObj, {
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
