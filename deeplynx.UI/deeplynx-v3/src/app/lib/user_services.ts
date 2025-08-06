import axios from "axios";

console.log('All environment variables:', {
    OKTA_ISSUER: process.env.NEXT_PUBLIC_OKTA_ISSUER,
    OKTA_CLIENT_ID: process.env.NEXT_PUBLIC_OKTA_CLIENT_ID,
    API_URL: process.env.NEXT_PUBLIC_API_URL,
    OKTA_CLIENT_SECRET: process.env.NEXT_PUBLIC_OKTA_CLIENT_SECRET,
    AUTH_SECRET: process.env.NEXT_PUBLIC_AUTH_SECRET,
    REDIRECT_LINK: process.env.NEXT_PUBLIC_REDIRECT_LINK,
    NODE_ENV: process.env.NODE_ENV,
});

const API_BASE_URL = !process.env.NEXT_PUBLIC_API_URL ? '/api' : process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({
    baseURL: API_BASE_URL,
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