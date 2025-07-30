import axios from "axios";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true,
})

export const getAllProjects = async () => {
    try {
        const res = await api.get("https://nexus.dev.inl.gov/api/projects/GetAllProjects");
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}

export const getProject = async (projectId: string) => {
    try {
        const res = await api.get(`/projects/GetProject/${projectId}`);
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}

export const getProjectStats = async (projectId: string) => {
    try {
        const res = await api.get(`/projects/ProjectStats/${projectId}`);
        return res.data;
    } catch (error) {
        console.error("API call failed:", error)
        throw error;
    }
}

export const createProject = async (data: {
    name: string;
    abbreviation: string | null;
    description: string | null;
}) => {
    const res = await api.post("/projects/CreateProject", data, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
}