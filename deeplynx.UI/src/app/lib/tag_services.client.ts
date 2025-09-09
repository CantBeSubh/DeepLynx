import axios from 'axios';
import { TagResponseDto } from '../(home)/types/types';

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true
})

// Duplicate but unsure of format we will go with
export async function getTagsForProjects(
    projectId: string, projectIds: string[]
): Promise<TagResponseDto[]> {
    const query = projectIds.map(id => `projectIds=${id}`).join("&");
    const res = await api.get(`/projects/${projectId}/tags/GetAllTags?${query}`);
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
