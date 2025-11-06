// src/app/lib/projects_services.server.ts

import { OrganizationResponseDto } from "../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";
import "server-only";

export async function getAllOrganizationsServer(): Promise<OrganizationResponseDto[]> {
    const res = await apiFetch("/organizations/GetAllOrganizations");
    return asJson<OrganizationResponseDto[]>(res)
}
