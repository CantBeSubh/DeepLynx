import { CreateOauthApplicationRequestDto, UpdateOauthApplicationRequestDto } from "../(home)/types/requestDTOs";
import { OauthApplicationResponseDto, OauthApplicationSecureResponseDto } from "../(home)/types/responseDTOs";
import api from "./api";

//List OAuth Applications
export const getAllOauthApplications = async (hideArchived: boolean = true): Promise<OauthApplicationResponseDto[]> => {
    try {
        const res = await api.get<OauthApplicationResponseDto[]>(
            `/oauth-applications/GetAllOauthApplications`,
            { params: { hideArchived } }
        );
        return res.data;
    } catch (error) {
        console.error("Error fetching OAuth applications:", error);
        throw error;
    }
};

// //Fetch OAuth Application by ID
// export const getOauthApplication = async (applicationId: number, hideArchived: boolean = true): Promise<OauthApplicationResponseDto> => {
//     try {
//         const res = await api.get<OauthApplicationResponseDto>(
//             `/oauth-applications/GetOauthApplication/${applicationId}`,
//             { params: { hideArchived } }
//         );
//         return res.data;
//     } catch (error) {
//         console.error(`Error fetching OAuth application ${applicationId}:`, error);
//         throw error;
//     }
// };

//Create an OAuth Application
// export const createOauthApplication = async (dto: CreateOauthApplicationRequestDto): Promise<OauthApplicationSecureResponseDto> => {
//     try {
//         const res = await api.post<OauthApplicationSecureResponseDto>(
//             `/oauth-applications/CreateOauthApplication`,
//             dto
//         );
//         return res.data;
//     } catch (error) {
//         console.error("Error creating OAuth application:", error);
//         throw error;
//     }
// };

export async function createOauthApplication(dto: CreateOauthApplicationRequestDto) {
    const res = await api.post("/oauth-applications/CreateOauthApplication", dto, {
        headers: { "Content-Type": "application/json" },
    });
    return res.data;
}

//Update an OAuth Application
export const updateOauthApplication = async (applicationId: number, dto: UpdateOauthApplicationRequestDto): Promise<OauthApplicationResponseDto> => {
    try {
        const res = await api.put<OauthApplicationResponseDto>(
            `/oauth-applications/UpdateOauthApplication/${applicationId}`,
            dto
        );
        return res.data;
    } catch (error) {
        console.error(`Error updating OAuth application ${applicationId}:`, error);
        throw error;
    }
};

//Archive an OAuth Application
export const archiveOauthApplication = async (applicationId: number): Promise<void> => {
    try {
        await api.delete(`/oauth-applications/ArchiveOauthApplication/${applicationId}`);
    } catch (error) {
        console.error(`Error archiving OAuth application ${applicationId}:`, error);
        throw error;
    }
};
