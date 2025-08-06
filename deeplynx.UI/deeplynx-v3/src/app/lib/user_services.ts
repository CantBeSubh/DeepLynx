import axios from "axios";

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true,
})

export const getDataOverview = async (userId: string) => {
    try {
        const res = await api.get(`/user/GetDataOverview/${userId}`);
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}

export const getRecentlyAddedRecords = async (projectIds: string[]) => {
    try {
        const queryString = projectIds
            .map(id => `projectId=${encodeURIComponent(id)}`)
            .join("&");
        const res = await api.get(`/user/GetRecentlyAddedRecords?${queryString}`);
        return res.data
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}