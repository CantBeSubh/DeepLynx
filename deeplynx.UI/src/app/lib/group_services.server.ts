// src/app/lib/group_services.server.ts

import "server-only"
import { GroupResponseDto } from "../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";

export async function getAllGroups(organizationId: number | string,
    hideArchived: boolean = true
): Promise<GroupResponseDto[]> {
    const params = new URLSearchParams();

    params.append('hideArchived', String(hideArchived));

    if (organizationId !== undefined) {
        params.append('organizationId', String(organizationId));
    }

    const res = await apiFetch(`/groups/GetAllGroups?${params.toString()}`);
    return asJson<GroupResponseDto[]>(res)
}