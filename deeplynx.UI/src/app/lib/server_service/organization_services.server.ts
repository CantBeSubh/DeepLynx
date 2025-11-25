// src/app/lib/projects_services.server.ts
import "server-only";
import { OrganizationResponseDto } from "../../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";

export async function getAllOrganizationsServer(
    hideArchived: boolean = true
): Promise<OrganizationResponseDto[]> {
    const params = new URLSearchParams();
    params.append('hideArchived', String(hideArchived));

    const res = await apiFetch(`/organizations?${params.toString()}`);
    return asJson<OrganizationResponseDto[]>(res);
}

export async function getOrganizationServer(
    organizationId: number,
    hideArchived: boolean = true
): Promise<OrganizationResponseDto> {
    const params = new URLSearchParams();
    params.append('hideArchived', String(hideArchived));

    const res = await apiFetch(`/organizations/${organizationId}?${params.toString()}`);
    return asJson<OrganizationResponseDto>(res);
}