import React from "react";
import SysAdminClient from "./SysAdminClient";
import { OauthApplicationResponseDto, OrganizationResponseDto } from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/organization_services.server";
import { getAllOauthApplicationsServer } from "@/app/lib/oauth_services.server";

type Props = {
  params: Promise<{ id?: string }>;
};

export default async function SysAdminPage({ params }: Props) {
  const OrganizationResponseDtos = (await getAllOrganizationsServer()) as OrganizationResponseDto[];
  const oAuthApplications = (await getAllOauthApplicationsServer()) as OauthApplicationResponseDto[];
  return (
    <SysAdminClient
      organizations={OrganizationResponseDtos}
      applications={oAuthApplications}
    />
  );
};
