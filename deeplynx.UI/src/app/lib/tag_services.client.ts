import { TagResponseDto } from '../(home)/types/types';
import api from './api';

export const getTag = async (projectId: number, tagId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/tags/GetTag/${tagId}`);
        return res.data;
    } catch (error) {
        console.error("Error getting a tag:", error);
        throw error;
    }
}
