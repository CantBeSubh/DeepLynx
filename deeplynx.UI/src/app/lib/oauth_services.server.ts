// src/app/lib/oauth_services.server.ts
import "server-only";
import { OauthApplicationResponseDto } from "../(home)/types/responseDTOs";
import { apiFetch, asJson } from "./api.server";

export async function getAllOauthApplicationsServer(
    hideArchived: boolean = true
): Promise<OauthApplicationResponseDto[]> {
    const params = new URLSearchParams();
    params.append('hideArchived', String(hideArchived));

    const res = await apiFetch(`/oauth/applications?${params.toString()}`);
    return asJson<OauthApplicationResponseDto[]>(res);
}