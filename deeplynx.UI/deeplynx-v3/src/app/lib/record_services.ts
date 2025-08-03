import axios from 'axios';

const API_BASE_URL = !process.env.NEXT_PUBLIC_API_URL ? '/api' : process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true
})

export const getAllRecords = async (projectId: string) => {
    try {
        const res = await api.get(`/projects/${projectId}/records/GetAllRecords`);
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}

export const getRecord = async (projectId: number, recordId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/records/GetRecord/${recordId}`);
        return res.data;
    } catch (error) {
        console.error("Error fetching record:", error);
        throw error;
    }
}

export const updateRecord = async (
    projectId: number, 
    recordId: number, 
    updateData: {
        uri?: string | null; 
        properties?: Record<string, any>;
        original_id?: string | null;
        name?: string | null;
        class_id?: number | null;
        class_name?: string | null;
        description?: string| null;
    }) => {
        try {
            const res = await api.put(
                `/projects/${projectId}/records/UpdateRecord/${recordId}`, 
                updateData, 
                {headers: {
                    "Content-Type": "application/json"
                }})
            return res.data;
        } catch (error) {
            console.error("Error updating record:", error);
            throw error;
        }
    }