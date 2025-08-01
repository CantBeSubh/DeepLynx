import axios from "axios";

const API_BASE_URL = !process.env.NEXT_PUBLIC_API_URL ? '/api' : process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true
})

export const filterRecords = async (filter: string[]) => {
    try {
        const res = await api.post("/records/Filter", filter, {
            headers: {"Content-Type": "application/json"},
        });
        return res.data;
    } catch (error) {
        console.error("Error filtering records:", error);
        throw error;
    }
}