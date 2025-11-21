'use client';

import api from './api';
import { ClassResponseDto } from '../(home)/types/responseDTOs';
import { CreateClassRequestDto, UpdateClassRequestDto } from '../(home)/types/requestDTOs';

/**
 * Get all classes for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param hideArchived - Flag to hide archived classes (default: true)
 * @returns Promise with array of ClassResponseDto
 */
export const getAllClasses = async (
    organizationId: number,
    projectId: number,
    hideArchived: boolean = true
): Promise<ClassResponseDto[]> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/classes`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error getting all classes:", error);
        throw error;
    }
};

/**
 * Get a specific class
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param classId - The ID of the class
 * @param hideArchived - Flag to hide archived classes (default: true)
 * @returns Promise with ClassResponseDto
 */
export const getClass = async (
    organizationId: number,
    projectId: number,
    classId: number,
    hideArchived: boolean = true
): Promise<ClassResponseDto> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/classes/${classId}`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error getting class ${classId}:`, error);
        throw error;
    }
};

/**
 * Get all classes from multiple projects
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project (required for route)
 * @param projectIds - Array of project IDs to retrieve classes from
 * @param hideArchived - Flag to hide archived classes (default: true)
 * @returns Promise with array of ClassResponseDto
 */
export const getAllClassesMultiProject = async (
    organizationId: number,
    projectId: number,
    projectIds: number[],
    hideArchived: boolean = true
): Promise<ClassResponseDto[]> => {
    try {
        const res = await api.get(
            `/organizations/${organizationId}/projects/${projectId}/classes/multiproject`,
            { params: { projectIds, hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error getting classes from multiple projects:", error);
        throw error;
    }
};

/**
 * Create a new class
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dto - The class creation request DTO
 * @returns Promise with ClassResponseDto
 */
export const createClass = async (
    organizationId: number,
    projectId: number,
    dto: CreateClassRequestDto
): Promise<ClassResponseDto> => {
    try {
        const res = await api.post(
            `/organizations/${organizationId}/projects/${projectId}/classes`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error("Error creating class:", error);
        throw error;
    }
};

/**
 * Bulk create classes
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param classes - Array of class creation request DTOs
 * @returns Promise with array of ClassResponseDto
 */
export const bulkCreateClasses = async (
    organizationId: number,
    projectId: number,
    classes: CreateClassRequestDto[]
): Promise<ClassResponseDto[]> => {
    try {
        const res = await api.post(
            `/organizations/${organizationId}/projects/${projectId}/classes/bulk`,
            classes
        );
        return res.data;
    } catch (error) {
        console.error("Error bulk creating classes:", error);
        throw error;
    }
};

/**
 * Update a class
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param classId - The ID of the class to update
 * @param dto - The class update request DTO
 * @returns Promise with ClassResponseDto
 */
export const updateClass = async (
    organizationId: number,
    projectId: number,
    classId: number,
    dto: UpdateClassRequestDto
): Promise<ClassResponseDto> => {
    try {
        const res = await api.put(
            `/organizations/${organizationId}/projects/${projectId}/classes/${classId}`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error(`Error updating class ${classId}:`, error);
        throw error;
    }
};

/**
 * Delete a class
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param classId - The ID of the class to delete
 * @returns Promise with success message
 */
export const deleteClass = async (
    organizationId: number,
    projectId: number,
    classId: number
): Promise<{ message: string }> => {
    try {
        const res = await api.delete(
            `/organizations/${organizationId}/projects/${projectId}/classes/${classId}`
        );
        return res.data;
    } catch (error) {
        console.error(`Error deleting class ${classId}:`, error);
        throw error;
    }
};

/**
 * Archive or unarchive a class
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param classId - The ID of the class to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export const archiveClass = async (
    organizationId: number,
    projectId: number,
    classId: number,
    archive: boolean
): Promise<{ message: string }> => {
    try {
        const res = await api.patch(
            `/organizations/${organizationId}/projects/${projectId}/classes/${classId}`,
            null,
            { params: { archive } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error ${archive ? 'archiving' : 'unarchiving'} class ${classId}:`, error);
        throw error;
    }
};