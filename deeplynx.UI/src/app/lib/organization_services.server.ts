// src/app/lib/projects_services.server.ts

import { OrganizationResponseDto } from "../(home)/types/responseDTOs";
import apiServer from "./api.server"

export async function getAllOrganizationsServer(): Promise<OrganizationResponseDto[]> {
    return apiServer.get<OrganizationResponseDto[]>("/organizations/GetAllOrganizations");
}