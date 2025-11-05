

import { CreateOrganizationRequestDto } from "../(home)/types/requestDTOs";
import { OrganizationResponseDto } from "../(home)/types/responseDTOs";
import api from "./api";
// In organization_services.client.ts


//List Organizations
export const getAllOrganizations = async (): Promise<OrganizationResponseDto[]> => {
    try {
        const res = await api.get<OrganizationResponseDto[]>(
            `/organizations/GetAllOrganizations`
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching organizations:", error);
        throw error;
    }
};
//Fetch Organization by ID

//Create an Organization
export async function createOrganization(dto: CreateOrganizationRequestDto) {
    try {
        const res = await api.post<OrganizationResponseDto>(
            `organizations/CreateOrganization`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error("Error creating organization:", error);
        throw error;
    }
};

// export async function createProject(data: {
//     name: string;
//     abbreviation: string | null;
//     description: string | null;
// }) {
//     const res = await api.post("/projects/CreateProject", data, {
//         headers: { "Content-Type": "application/json" },
//     });
//     return res.data;
// }


//Update an Organization

//Delete an Organization

//Archive an Organization

//Add User to Organization

//Set Admin Status to Organization User

//Remove User from Organization