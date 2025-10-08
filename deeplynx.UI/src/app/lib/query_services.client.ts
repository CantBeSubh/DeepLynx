// src/app/lib/query_services.client.ts
"use client";
import { ClassResponseDto } from '../(home)/types/responseDTOs/classResponseDto';
import { CustomQueryRequestDto } from '../(home)/types/requestDTOs/customRequestDTO';
import api from "./api";
import { FileViewerTableRow } from '../(home)/types/types';
import { DataSourceResponseDto } from '../(home)/types/responseDTOs/dataSourceResponseDto';
import { TagResponseDto } from '../(home)/types/responseDTOs/tagResponseDto';

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function queryBuilder(
    queryObj: CustomQueryRequestDto[],
    fullTextSearch: string | null, projectIds: string[]
) {
    //Building json string format from key/value input
    for (const obj of queryObj) {
        if (obj.jsonKey && obj.jsonValue) {
            const json = `{"${obj.jsonKey}": "${obj.jsonValue}"}`
            obj.json = json;
        }
    }
    const newProjectIds = projectIds.map((str) => Number(str));
    const projectIdsQuery = newProjectIds.map(id => `projectIds=${id}`).join('&');
    const query = (`${projectIdsQuery}&textSearch=${fullTextSearch}`);
    const res = await api.post(`/records/QueryBuilder?${query}`, queryObj, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
}

export async function fullTextSearch(
    userQuery: string, projectIds: string[]
): Promise<FileViewerTableRow[]> {
    const newProjectIds = projectIds.map((str) => Number(str));
    const projectIdsQuery = newProjectIds.map(id => `projectIds=${id}`).join('&');
    const res = await api.get(`/records/Filter?userQuery=${userQuery}&${projectIdsQuery}`);
    return res.data as FileViewerTableRow[];
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
