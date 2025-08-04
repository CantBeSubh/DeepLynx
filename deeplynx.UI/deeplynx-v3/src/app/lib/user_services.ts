import axios from "axios";

const API_BASE_URL = !process.env.NEXT_PUBLIC_API_URL ? '/api' : process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true,
})

export const getDataOverview = async (userId: string) => {
    console.log(process.env.NEXT_PUBLIC_API_URL, process.env.NEXT_PUBLIC_OKTA_ISSUER)
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