// src/app/(home)/organization_management/page.tsx

import React from "react";
import OrganizationManagmentClient from "./OrganizationManagementClient";
import {
  GroupResponseDto,
  OauthApplicationResponseDto,
  OrganizationResponseDto,
  ProjectResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import { getAllOrganizationsServer } from "@/app/lib/organization_services.server";
import { getAllOauthApplicationsServer } from "@/app/lib/oauth_services.server";
import { getAllUsersServer } from "@/app/lib/user_services.server";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { mapToProjectResponseDtos } from "../page";
import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import { getAllGroups } from "@/app/lib/group_services.server";

export const dynamic = "force-dynamic";

// Mapping function for groups (if needed)
const mapToGroupResponseDtos = (group: any): GroupResponseDto => {
  return {
    ...group,
    // Add any necessary transformations here
  } as GroupResponseDto;
};

const OrganizationManagementPage = async ({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) => {
  // Get organization from cookies
  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

  // Check if cookie exists before accessing its value
  if (!orgSessionCookie) {
    redirect("/select-org");
  }

  let organizationId: string | number;
  try {
    const orgSession = JSON.parse(orgSessionCookie.value);
    organizationId = orgSession.organizationId;
  } catch (e) {
    console.error("Failed to parse organization session:", e);
    redirect("/select-org");
  }

  // Fetch projects filtered by organization
  let projects: ProjectResponseDto[] = [];
  try {
    const apiProjects = await getAllProjectsServer(organizationId, true);
    projects = apiProjects.map(mapToProjectResponseDtos);
  } catch (e) {
    console.error("getAllProjectsServer failed:", e);
  }

  // Fetch groups filtered by organization
  let groups: GroupResponseDto[] = [];
  try {
    const apiGroups = await getAllGroups(organizationId, true);
    groups = apiGroups.map(mapToGroupResponseDtos);
  } catch (error) {
    console.error("getAllGroups failed:", error);
  }

  // Fetch users filtered by organization
  const members = (await getAllUsersServer(
    undefined,
    Number(organizationId)
  )) as UserResponseDto[];

  const params = await searchParams;
  const fromProject =
    typeof params.fromProject === "string" ? params.fromProject : "";

  // Find the initial selected project or use the first one
  const initialSelectedProject = fromProject
    ? projects.find((p) => String(p.id) === fromProject) || projects[0]
    : projects[0];

  return (
    <OrganizationManagmentClient
      members={members}
      initialProjects={projects}
      initialGroups={groups}
      initialSelectedProject={initialSelectedProject}
    />
  );
};

export default OrganizationManagementPage;
