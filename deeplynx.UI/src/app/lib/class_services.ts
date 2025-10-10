import axios from 'axios';

export const api = axios.create({
    baseURL: process.env.API_URL,
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

