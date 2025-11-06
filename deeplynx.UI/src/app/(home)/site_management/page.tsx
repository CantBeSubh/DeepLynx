import React from "react";
import SysAdminClient from "./SysAdminClient";
import { OauthApplicationResponseDto, OrganizationResponseDto, UserResponseDto } from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/organization_services.server";
import { getAllOauthApplicationsServer } from "@/app/lib/oauth_services.server";
import { getAllUsersServer } from "@/app/lib/user_services.server";

type Props = {
  params: Promise<{ id?: string }>;
};

export default async function SysAdminPage({ params }: Props) {
  const OrganizationResponseDtos = (await getAllOrganizationsServer()) as OrganizationResponseDto[];
  const oAuthApplications = (await getAllOauthApplicationsServer()) as OauthApplicationResponseDto[];
  const members = (await getAllUsersServer()) as UserResponseDto[];
  return (
    <SysAdminClient
      organizations={OrganizationResponseDtos}
      applications={oAuthApplications}
      members={members}
    />
  );
};
