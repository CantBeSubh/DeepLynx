import React from "react";
import SysAdminClient from "./SysAdminClient";
import { OrganizationResponseDto } from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/organization_services.server";

type Props = {
  params: Promise<{ id?: string }>;
};

export default async function SysAdminPage({ params }: Props) {
  const OrganizationResponseDtos = (await getAllOrganizationsServer()) as OrganizationResponseDto[];
  console.log(OrganizationResponseDtos)
  return (
    <SysAdminClient
      organizations={OrganizationResponseDtos}
    />
  );
};
