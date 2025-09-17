'use client';

import { DataSourceResponseDto } from '../(home)/types/types';
import api from './api';

export async function getDataSourcesForProjects(projectId: string,
    projectIds: string[]
): Promise<DataSourceResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/projects/${projectId}/datasources/GetAllDataSources?${query}`);
    return res.data as DataSourceResponseDto[];
}
