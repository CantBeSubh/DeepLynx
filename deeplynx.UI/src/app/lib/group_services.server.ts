// src/app/lib/group_services.server.ts

import "server-only"
import { GroupResponseDto } from "../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";

export async function getAllGroups(
    organizationId: number,
    hideArchived: boolean = true
): Promise<GroupResponseDto[]> {
    const params = new URLSearchParams();
    params.append('hideArchived', String(hideArchived));

    const res = await apiFetch(
        `/organizations/${organizationId}/groups?${params.toString()}`
    );
    return asJson<GroupResponseDto[]>(res);
}