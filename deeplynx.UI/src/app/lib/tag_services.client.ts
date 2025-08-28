import axios from 'axios';
import { TagResponseDto } from '../(home)/types/types';

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true
})

export const getAllTags = async (projectId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/tags/GetAllTags`);
        return res.data;
    } catch (error) {
        console.error("Error getting all tags:", error);
        throw error;
    }
}

// Duplicate but unsure of format we will go with
export async function getTagsForProject(
    projectId?: string
): Promise<TagResponseDto[]> {
    const res = await api.get(`/projects/${projectId}/tags/GetAllTags`);
    return res.data as TagResponseDto[];
}

export const getTag = async (projectId: number, tagId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/tags/GetTag/${tagId}`);
        return res.data;
    } catch (error) {
        console.error("Error getting a tag:", error);
        throw error;
    }
}
