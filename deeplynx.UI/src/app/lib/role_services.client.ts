import api from './api';


export const getAllRoles = async (
    projectId?: number,
    organizationId?: number,
    hideArchived: boolean = true
) => {
    try {
        const res = await api.get('/roles/GetAllRoles', {
            params: {
                projectId,
                organizationId,
                hideArchived
            }
        });
        return res.data;
    } catch (error) {
        console.error("Error getting all roles:", error);
        throw error;
    }
}