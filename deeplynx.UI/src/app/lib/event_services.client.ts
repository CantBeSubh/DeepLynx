'use client';

import api from './api';
import { EventResponseDto, PaginatedEventsResponseDto } from '../(home)/types/responseDTOs';
import { EventsQueryRequestDTO, CreateEventRequestDto } from '../(home)/types/requestDTOs';
import { PaginatedResponse } from '../(home)/types/types';


/**
 * Get all events for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @returns Promise with array of EventResponseDto
 */
export const getAllEvents = async (
    organizationId: number,
    projectId: number
): Promise<EventResponseDto[]> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/events`
        );
        return res.data;
    } catch (error) {
        console.error("Error getting all events:", error);
        throw error;
    }
};

/**
 * Query events with pagination and filters
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param queryDto - Filter criteria and pagination parameters
 * @returns Promise with paginated response of EventResponseDto
 */
export const queryEvents = async (
    organizationId: number,
    projectId: number,
    queryDto?: EventsQueryRequestDTO
): Promise<PaginatedResponse<EventResponseDto>> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/events/query`,
            {
                params: {
                    pageNumber: queryDto?.pageNumber || 1,
                    pageSize: queryDto?.pageSize || 10,
                    ...queryDto
                }
            }
        );
        return res.data;
    } catch (error) {
        console.error("Error querying events:", error);
        throw error;
    }
};

/**
 * Query events by user project membership with pagination and filters
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param queryDto - Filter criteria and pagination parameters
 * @returns Promise with paginated response of EventResponseDto for projects the user is a member of
 */
export const queryEventsByUser = async (
    organizationId: number,
    projectId: number,
    queryDto?: EventsQueryRequestDTO
): Promise<PaginatedResponse<EventResponseDto>> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/events/by-user`,
            {
                params: {
                    pageNumber: queryDto?.pageNumber || 1,
                    pageSize: queryDto?.pageSize || 10,
                    ...queryDto
                }
            }
        );
        return res.data;
    } catch (error) {
        console.error("Error querying events by user:", error);
        throw error;
    }
};

/**
 * Get events by user project subscriptions
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param userId - The ID of the user whose subscriptions to filter by
 * @returns Promise with array of EventResponseDto
 */
export const getEventsByUserSubscriptions = async (
    organizationId: number,
    projectId: number,
    userId: number
): Promise<EventResponseDto[]> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/events/subscriptions`,
            { params: { userId } }
        );
        return res.data;
    } catch (error) {
        console.error("Error getting events by subscriptions:", error);
        throw error;
    }
};

/**
 * Create a new event
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dto - The event creation request DTO
 * @returns Promise with EventResponseDto
 */
export const createEvent = async (
    organizationId: number,
    projectId: number,
    dto: CreateEventRequestDto
): Promise<EventResponseDto> => {
    try {
        const res = await api.post(
            `/organizations/${organizationId}/projects/${projectId}/events`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error("Error creating event:", error);
        throw error;
    }
};

/**
 * Bulk create events
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param events - Array of event creation request DTOs
 * @returns Promise with array of EventResponseDto
 */
export const bulkCreateEvents = async (
    organizationId: number,
    projectId: number,
    events: CreateEventRequestDto[]
): Promise<EventResponseDto[]> => {
    try {
        const res = await api.post(
            `/organizations/${organizationId}/projects/${projectId}/events/bulk`,
            events
        );
        return res.data;
    } catch (error) {
        console.error("Error bulk creating events:", error);
        throw error;
    }
};