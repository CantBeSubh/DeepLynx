'use client';

import axios from 'axios';
import { DataSourceResponseDto } from '../(home)/types/types';

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true
})


export async function getDataSourcesForProject(
    projectId?: string
): Promise<DataSourceResponseDto[]> {
    const res = await api.get(`/projects/${projectId}/datasources/GetAllDataSources`);
    return res.data as DataSourceResponseDto[];
}

