import { api } from "./get_apis";


export const createProject = async (data: {
    name: string;
    abbreviation: string | null;
    description: string | null;
}) => {
    const res = await api.post("/projects/CreateProject", data, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
};
