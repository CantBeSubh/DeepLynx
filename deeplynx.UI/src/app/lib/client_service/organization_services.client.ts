import { CreateOrganizationRequestDto, UpdateOrganizationRequestDto } from "@/app/(home)/types/requestDTOs";
import { OrganizationResponseDto } from "@/app/(home)/types/responseDTOs";
import api from "./api";


/**
 * Get all organizations
 * @param hideArchived - Flag to hide archived organizations (default: true)
 * @returns Promise with array of OrganizationResponseDto
 */
export const getAllOrganizations = async (
    hideArchived: boolean = true
): Promise<OrganizationResponseDto[]> => {
    try {
        const res = await api.get<OrganizationResponseDto[]>(
            `/organizations`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching organizations:", error);
        throw error;
    }
};

/**
 * Get all organizations for the current user
 * @param hideArchived - Flag to hide archived organizations (default: true)
 * @returns Promise with array of OrganizationResponseDto
 */
export const getAllOrganizationsForUser = async (
    hideArchived: boolean = true
): Promise<OrganizationResponseDto[]> => {
    try {
        const res = await api.get<OrganizationResponseDto[]>(
            `/organizations/user`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching organizations for user:", error);
        throw error;
    }
};

/**
 * Get a specific organization by ID
 * @param organizationId - The ID of the organization
 * @param hideArchived - Flag to hide archived organizations (default: true)
 * @returns Promise with OrganizationResponseDto
 */
export const getOrganization = async (
    organizationId: number,
    hideArchived: boolean = true
): Promise<OrganizationResponseDto> => {
    try {
        const res = await api.get<OrganizationResponseDto>(
            `/organizations/${organizationId}`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error fetching organization ${organizationId}:`, error);
        throw error;
    }
};

/**
 * Create a new organization
 * @param dto - The organization creation request DTO
 * @returns Promise with OrganizationResponseDto
 */
export const createOrganization = async (
    dto: CreateOrganizationRequestDto
): Promise<OrganizationResponseDto> => {
    try {
        const res = await api.post<OrganizationResponseDto>(
            `/organizations`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error("Error creating organization:", error);
        throw error;
    }
};

/**
 * Update an organization
 * @param organizationId - The ID of the organization to update
 * @param dto - The organization update request DTO
 * @returns Promise with OrganizationResponseDto
 */
export const updateOrganization = async (
    organizationId: number,
    dto: UpdateOrganizationRequestDto
): Promise<OrganizationResponseDto> => {
    try {
        const res = await api.put<OrganizationResponseDto>(
            `/organizations/${organizationId}`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error(`Error updating organization ${organizationId}:`, error);
        throw error;
    }
};

/**
 * Delete an organization
 * @param organizationId - The ID of the organization to delete
 * @returns Promise with success message
 */
export const deleteOrganization = async (
    organizationId: number
): Promise<{ message: string }> => {
    try {
        const res = await api.delete(
            `/organizations/${organizationId}`
        );
        return res.data;
    } catch (error) {
        console.error(`Error deleting organization ${organizationId}:`, error);
        throw error;
    }
};

/**
 * Archive or unarchive an organization
 * @param organizationId - The ID of the organization to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export const archiveOrganization = async (
    organizationId: number,
    archive: boolean = true
): Promise<{ message: string }> => {
    try {
        const res = await api.patch(
            `/organizations/${organizationId}`,
            null,
            { params: { archive } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error ${archive ? 'archiving' : 'unarchiving'} organization ${organizationId}:`, error);
        throw error;
    }
};

/**
 * Add a user to an organization
 * @param organizationId - The ID of the organization
 * @param userId - The ID of the user to add
 * @param isAdmin - Whether to add the user as an admin (default: false)
 * @returns Promise with success message
 */
export const addUserToOrganization = async (
    organizationId: number,
    userId: number,
    isAdmin: boolean = false
): Promise<{ message: string }> => {
    try {
        const res = await api.post(
            `/organizations/${organizationId}/user`,
            null,
            { params: { userId, isAdmin } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error adding user ${userId} to organization ${organizationId}:`, error);
        throw error;
    }
};

/**
 * Set admin status for an organization user
 * @param organizationId - The ID of the organization
 * @param userId - The ID of the user
 * @param isAdmin - The admin status to set
 * @returns Promise with success message
 */
export const setOrganizationAdminStatus = async (
    organizationId: number,
    userId: number,
    isAdmin: boolean
): Promise<{ message: string }> => {
    try {
        const res = await api.put(
            `/organizations/${organizationId}/admin`,
            null,
            { params: { userId, isAdmin } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error setting admin status for user ${userId} in organization ${organizationId}:`, error);
        throw error;
    }
};

/**
 * Remove a user from an organization
 * @param organizationId - The ID of the organization
 * @param userId - The ID of the user to remove
 * @returns Promise with success message
 */
export const removeUserFromOrganization = async (
    organizationId: number,
    userId: number
): Promise<{ message: string }> => {
    try {
        const res = await api.delete(
            `/organizations/${organizationId}/user`,
            { params: { userId } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error removing user ${userId} from organization ${organizationId}:`, error);
        throw error;
    }
};