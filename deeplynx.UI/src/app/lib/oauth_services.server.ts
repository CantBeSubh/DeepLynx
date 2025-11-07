// src/app/lib/oauth_services.server.ts
import "server-only";
import { OauthApplicationResponseDto } from "../(home)/types/responseDTOs";
import { auth } from "../../../auth";
import { apiFetch, asJson } from "./api.server";

export async function getAllOauthApplicationsServer(): Promise<OauthApplicationResponseDto[]> {
    const res = await apiFetch("/oauth-applications/GetAllOauthApplications");
    return asJson<OauthApplicationResponseDto[]>(res);
}


