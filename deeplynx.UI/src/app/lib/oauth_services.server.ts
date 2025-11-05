import { OauthApplicationResponseDto } from "../(home)/types/responseDTOs";
import apiServer from "./api.server"

export async function getAllOauthApplicationsServer(): Promise<OauthApplicationResponseDto[]> {
    return apiServer.get<OauthApplicationResponseDto[]>("/oauth-applications/GetAllOauthApplications");
}
