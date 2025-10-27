'use client';

import api from './api';
import { EventResponseDto, PaginatedEventsResponse } from "../(home)/types/responseDTOs"

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
    params?: EventFilterParams
): Promise<PaginatedEventsResponse> => {
    try {
        const res = await api.get(`/events/GetAllEventsPaginated`, {
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
    params?: EventFilterParams
): Promise<PaginatedEventsResponse> => {
    try {
        const res = await api.get(`/events/GetAllEventsByUserPaginated`, {
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
    params?: Omit<EventFilterParams, 'pageNumber' | 'pageSize'>
): Promise<EventResponseDto[]> => {
    try {
        const res = await api.get(`/events/GetAllEvents`, {
            params,
        });
        return res.data;
    } catch (error) {
        console.error("Error getting all events:", error);
        throw error;
    }
};