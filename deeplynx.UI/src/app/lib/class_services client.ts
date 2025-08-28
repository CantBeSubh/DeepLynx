'use client';

import axios from 'axios';
import { ClassResponseDto } from '../(home)/types/types';

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true
})

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
