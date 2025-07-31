import axios from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true
})
export const getAllRecords = async (projectId: string) => {
    console.log("Project ID:", projectId)
    try {
        const res = await api.get(`/projects/${projectId}/records/GetAllRecords`);
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}