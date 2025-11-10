import React from "react";
import OrganizationManagmentClient from "./OrganizationManagementClient";
import {
  OauthApplicationResponseDto,
  OrganizationResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/organization_services.server";
import { getAllOauthApplicationsServer } from "@/app/lib/oauth_services.server";
import { getAllUsersServer } from "@/app/lib/user_services.server";


export const dynamic = "force-dynamic";

const OrganizationManagementPage = async ({ }) => {
  const OrganizationResponseDtos = (await getAllOrganizationsServer()) as OrganizationResponseDto[];
  const oAuthApplications = (await getAllOauthApplicationsServer()) as OauthApplicationResponseDto[];
  const members = (await getAllUsersServer()) as UserResponseDto[];
  //TODO: get current organization and pass name to settings page 

  return (
    <OrganizationManagmentClient
      organizations={OrganizationResponseDtos}
      applications={oAuthApplications}
      members={members}
    />
  );
};

export default OrganizationManagementPage;
