'use client';

import { ClassResponseDto } from '../(home)/types/types';
import api from './api';

export const getClass = async (projectId: number, classId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/classes/GetClass/${classId}`);
        return res.data;
    } catch (error) {
        console.error("Error getting all tags:", error);
        throw error;
    }
}

export async function getClassesForProjects(projectId: string,
    projectIds: string[],
): Promise<ClassResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/projects/${projectId}/classes/GetAllClasses?${query}`);
    return res.data as ClassResponseDto[];
}
