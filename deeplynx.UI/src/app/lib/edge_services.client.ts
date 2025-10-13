import { RelatedRecordsRequestDto } from '../(home)/types/RelatedRecordsRequestDto';
import { RelatedRecordsResponseDto } from '../(home)/types/RelatedRecordsResponseDto';
import api from './api';

export const deleteEdge = async (projectId: number, edgeId: string | null, originId: string, destinationId: string) => {
    try {
        const queryParams = new URLSearchParams();
        if (edgeId) queryParams.append('edgeId', edgeId);
        if (originId) queryParams.append('originId', originId);
        if (destinationId) queryParams.append('destinationId', destinationId);

        const res = await api.delete(`/projects/${projectId}/edges/DeleteEdge`, {
            params: queryParams
        });
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
};

export const archiveEdge = async (
    projectId: number, 
    edgeId?: number | null, 
    originId?: number | null, 
    destinationId?: number | null
) => {
    try {
        const queryParams = new URLSearchParams();
        if (edgeId) queryParams.append('edgeId', edgeId.toString());
        if (originId) queryParams.append('originId', originId.toString());
        if (destinationId) queryParams.append('destinationId', destinationId.toString());

        const res = await api.delete(`/projects/${projectId}/edges/ArchiveEdge`, {
            params: queryParams
        });
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
};

export const getEdge = async (projectId: number, edgeId: string | null, originId: string, destinationId: string) => {
    try {
        const queryParams = new URLSearchParams();
        if (edgeId) queryParams.append('edgeId', edgeId);
        if (originId) queryParams.append('originId', originId);
        if (destinationId) queryParams.append('destinationId', destinationId);

        const res = await api.get(`/projects/${projectId}/edges/GetEdge`, {
            params: queryParams
        });
        return res.data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
};

export const getEdgesByRecord = async (
    projectId: string,
    params: RelatedRecordsRequestDto = {}
): Promise<RelatedRecordsResponseDto[]> => {
    try {
        const { data } = await api.get<RelatedRecordsResponseDto[]>(
            `/projects/${projectId}/edges/GetAllEdgesByRecord`,
            {
                params: {
                    recordId: params.recordId,
                    isOrigin: params.isOrigin,
                    page: params.page,
                    pageSize: params.pageSize ?? 20, // Default from API docs
                    hideArchived: params.hideArchived ?? true // Default from API docs
                }
            }
        );
        return data;
    } catch (error) {
        console.error("API call failed:", error);
        throw error;
    }
};
