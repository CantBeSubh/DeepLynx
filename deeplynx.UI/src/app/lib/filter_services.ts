import axios from "axios";

export const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true
})

export const filterRecords = async (filter: string) => {
    try {
        const res = await api.post("/records/Filter", filter, {
            headers: { "Content-Type": "application/json" },
        });
        return res.data;
    } catch (error) {
        console.error("Error filtering records:", error);
        throw error;
    }
}

export const queryRecords = async (query: string) => {
    const queryParams = {
        userQuery: query
    };
    try {
        const res = await api.get("/records/Filter/", { params: queryParams });
        return res.data;
    } catch (error) {
        console.error("Query records failed:", error);
        throw error;
    }
}