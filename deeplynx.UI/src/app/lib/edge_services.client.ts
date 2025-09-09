import axios from 'axios';
import api from './api';

// export const api = axios.create({
//     baseURL: process.env.NEXT_PUBLIC_API_URL,
//     withCredentials: true
// });

export const deleteEdge = async (projectId: number, edgeId: string | null, originId: string, destinationId: string) => {
    try {
        const queryParams = new URLSearchParams();
        if (edgeId) queryParams.append('edgeId', edgeId);
        if (originId) queryParams.append('originId', originId);
        if (destinationId) queryParams.append('destinationId', destinationId);

        const res = await api.delete(`/projects/${projectId}/edges/DeleteEdge`, {
            params: queryParams
        });
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
};

export const getEdge = async (projectId: number, edgeId: string | null, originId: string, destinationId: string) => {
    try {
        const queryParams = new URLSearchParams();
        if (edgeId) queryParams.append('edgeId', edgeId);
        if (originId) queryParams.append('originId', originId);
        if (destinationId) queryParams.append('destinationId', destinationId);

        const res = await api.get(`/projects/${projectId}/edges/GetEdge`, {
            params: queryParams
        });
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
};