'use client';

import axios from 'axios';
import { DataSourceResponseDto } from '../(home)/types/types';

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true
})


export async function getDataSourcesForProjects(projectId: string,
    projectIds: string[]
): Promise<DataSourceResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/projects/${projectId}/datasources/GetAllDataSources?${query}`);
    return res.data as DataSourceResponseDto[];
}
