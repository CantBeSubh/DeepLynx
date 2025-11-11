

import { CreateOrganizationRequestDto, UpdateOrganizationRequestDto } from "../(home)/types/requestDTOs";
import { OrganizationResponseDto } from "../(home)/types/responseDTOs";
import api from "./api";

//List Organizations
export const getAllOrganizations = async (hideArchived: boolean = true): Promise<OrganizationResponseDto[]> => {
    try {
        const res = await api.get<OrganizationResponseDto[]>(
            `/organizations/GetAllOrganizations`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching organizations:", error);
        throw error;
    }
};

//Fetch Organization by ID
export const getOrganization = async (organizationId: number, hideArchived: boolean = true): Promise<OrganizationResponseDto> => {
    try {
        const res = await api.get<OrganizationResponseDto>(
            `/organizations/GetOrganization/${organizationId}`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error(`Error fetching organization ${organizationId}:`, error);
        throw error;
    }
};

//Create an Organization
export const createOrganization = async (dto: CreateOrganizationRequestDto): Promise<OrganizationResponseDto> => {
    try {
        const res = await api.post<OrganizationResponseDto>(
            `/organizations/CreateOrganization`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error("Error creating organization:", error);
        throw error;
    }
};

//Update an Organization
export const updateOrganization = async (organizationId: number, dto: UpdateOrganizationRequestDto): Promise<OrganizationResponseDto> => {
    try {
        const res = await api.put<OrganizationResponseDto>(
            `/organizations/UpdateOrganization/${organizationId}`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error(`Error updating organization ${organizationId}:`, error);
        throw error;
    }
};

//Archive an Organization
export const archiveOrganization = async (organizationId: number): Promise<void> => {
    try {
        await api.delete(`/organizations/ArchiveOrganization/${organizationId}`);
    } catch (error) {
        console.error(`Error archiving organization ${organizationId}:`, error);
        throw error;
    }
};

// //Unarchive an Organization
// export const unarchiveOrganization = async (organizationId: number): Promise<void> => {
//     try {
//         await api.put(`/organizations/UnarchiveOrganization/${organizationId}`);
//     } catch (error) {
//         console.error(`Error unarchiving organization ${organizationId}:`, error);
//         throw error;
//     }
// };

//Add User to Organization
export const addUserToOrganization = async (organizationId: number, userId: number, isAdmin: boolean = false): Promise<void> => {
    try {
        await api.post(
            `/organizations/AddUserToOrganization`,
            null,
            { params: { organizationId, userId, isAdmin } }
        );
    } catch (error) {
        console.error(`Error adding user ${userId} to organization ${organizationId}:`, error);
        throw error;
    }
};

//Set Admin Status to Organization User
export const setOrganizationAdminStatus = async (organizationId: number, userId: number, isAdmin: boolean): Promise<void> => {
    try {
        await api.put(
            `/organizations/SetOrganizationAdminStatus`,
            null,
            { params: { organizationId, userId, isAdmin } }
        );
    } catch (error) {
        console.error(`Error setting admin status for user ${userId} in organization ${organizationId}:`, error);
        throw error;
    }
};

//Remove User from Organization
export const removeUserFromOrganization = async (organizationId: number, userId: number): Promise<void> => {
    try {
        await api.delete(
            `/organizations/RemoveUserFromOrganization`,
            { params: { organizationId, userId } }
        );
    } catch (error) {
        console.error(`Error removing user ${userId} from organization ${organizationId}:`, error);
        throw error;
    }
};