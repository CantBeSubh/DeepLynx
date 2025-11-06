import React from "react";
import SysAdminClient from "./SysAdminClient";
import {
  OauthApplicationResponseDto,
  OrganizationResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/organization_services.server";
import { getAllOauthApplicationsServer } from "@/app/lib/oauth_services.server";
import { getAllUsersServer } from "@/app/lib/user_services.server";
export const dynamic = "force-dynamic";

const SysAdminPage = async () => {
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
