import axios from "axios";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

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
    console.log("Sending project IDs: ", projectIds)
    try {
        console.log("requestBody: ", projectIds)
        const res = await api.post("/user/GetRecentlyAddedRecords", 
            projectIds, 
            {
            headers: {
                "Content-Type": "application/json"
            }
        });
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}