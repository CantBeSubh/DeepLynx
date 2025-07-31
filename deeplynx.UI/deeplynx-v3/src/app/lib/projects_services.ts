import axios from "axios";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export const api = axios.create({ withCredentials: true })

export const getAllProjects = async () => {
    try {
        console.log(api)
        const res = await api.get(API_BASE_URL + "/projects/GetAllProjects");
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}

export const getProject = async (projectId: string) => {
    try {
        const res = await api.get(API_BASE_URL + `/projects/GetProject/${projectId}`);
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
}

export const getProjectStats = async (projectId: string) => {
    try {
        const res = await api.get(API_BASE_URL + `/projects/ProjectStats/${projectId}`);
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
    const res = await api.post(API_BASE_URL + "/projects/CreateProject", data, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
}