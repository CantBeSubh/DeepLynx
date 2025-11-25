import React from "react";
import SysAdminClient from "./SysAdminClient";
import {
  OauthApplicationResponseDto,
  OrganizationResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/server_service/organization_services.server";
import { getAllOauthApplicationsServer } from "@/app/lib/server_service/oauth_services.server";
import { getAllUsersServer } from "@/app/lib/server_service/user_services.server";

export const dynamic = "force-dynamic";

const SysAdminPage = async ({}) => {
  const OrganizationResponseDtos =
    (await getAllOrganizationsServer()) as OrganizationResponseDto[];
  const oAuthApplications =
    (await getAllOauthApplicationsServer()) as OauthApplicationResponseDto[];
  const members = (await getAllUsersServer()) as UserResponseDto[];
  return (
    <SysAdminClient
      organizations={OrganizationResponseDtos}
      applications={oAuthApplications}
      members={members}
    />
  );
};

export default SysAdminPage;
