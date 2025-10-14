import axios from 'axios';

export const api = axios.create({
    baseURL: "/api",
    withCredentials: true
})

export const getNodesWithinDepth = async (
    projectId: number, 
    request: {
        tablename: string | undefined,
        id: number | undefined,
        depth: number
    }) => {
    try {
        const res = await api.post(
            `/projects/${projectId}/graph/Query-N-Layers`, 
            request,
            {
                headers: {
                    "Content-Type": "application/json"
                }
            });
        return res.data;
    } catch (error) {
        console.error("Error fetching record:", error);
        throw error;
    }
}

export const queryKuzu = async (projectId: number, request: string) => {
    try {
        const payload = {
            Query: request
        };

        const res = await api.post(
            `/projects/${projectId}/graph/Query`, 
            payload,
            {
                headers: {
                    "Content-Type": "application/json"
                }
            }
        );

        return res.data;
    } catch (error) {
        console.error("Error querying Kuzu:", error);
        throw error;
    }
}
