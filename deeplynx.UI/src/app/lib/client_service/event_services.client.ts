'use client';
import api from './client_service/api';
import { EventResponseDto, PaginatedEventsResponseDto } from "../(home)/types/responseDTOs"

export type EventFilterParams = {
    pageNumber?: number;
    pageSize?: number;
    projectId?: number;
    organizationId?: number;
    projectName?: string;
    lastUpdatedBy?: string;
    operation?: string;
    entityType?: string;
    entityName?: string;
    dataSourceName?: string;
    startDate?: string;
    endDate?: string;
};

export const getAllEventsPaginated = async (
    organizationId: number,
    projectId: number,
    params?: EventFilterParams
): Promise<PaginatedEventsResponseDto> => {
    try {
        const res = await api.get(`/organizations/${organizationId}/projects/${projectId}/events/query`, {
            params: {
                pageNumber: params?.pageNumber || 1,
                pageSize: params?.pageSize || 10,
                ...params,
            },
        });
        return res.data;
    } catch (error) {
        console.error("Error getting paginated events:", error);
        throw error;
    }
};

export const getAllEventsByUserPaginated = async (
    organizationId: number,
    projectId: number,
    params?: EventFilterParams
): Promise<PaginatedEventsResponseDto> => {
    try {
        const res = await api.get(`/organizations/${organizationId}/projects/${projectId}/events/by-user`, {
            params: {
                pageNumber: params?.pageNumber || 1,
                pageSize: params?.pageSize || 10,
                ...params,
            },
        });
        return res.data;
    } catch (error) {
        console.error("Error getting user events paginated:", error);
        throw error;
    }
};

export const getAllEvents = async (
    organizationId: number,
    projectId: number,
    params?: Omit<EventFilterParams, 'pageNumber' | 'pageSize'>
): Promise<EventResponseDto[]> => {
    try {
        const res = await api.get(`/organizations/${organizationId}/projects/${projectId}/events`, {
            params,
        });
        return res.data;
    } catch (error) {
        console.error("Error getting all events:", error);
        throw error;
    }
};