// src/app/lib/projects_services.server.ts
import "server-only";
import { OrganizationResponseDto } from "../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";


export async function getAllOrganizationsServer(): Promise<OrganizationResponseDto[]> {
    const res = await apiFetch("/organizations/GetAllOrganizations");
    return asJson<OrganizationResponseDto[]>(res)
}

// export async function getOrganizationServer(): Promise<OrganizationResponseDto[]> {
//     const res = await apiFetch("/organizations/GetAllOrganizations");
//     return asJson<OrganizationResponseDto[]>(res)
// }
