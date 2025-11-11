import React from "react";
import OrganizationManagmentClient from "./OrganizationManagementClient";
import {
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

export const dynamic = "force-dynamic";

const OrganizationManagementPage = async ({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) => {
  const OrganizationResponseDtos =
    (await getAllOrganizationsServer()) as OrganizationResponseDto[];
  const oAuthApplications =
    (await getAllOauthApplicationsServer()) as OauthApplicationResponseDto[];
  const members = (await getAllUsersServer()) as UserResponseDto[];

  // Get organization from cookies
  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

  // Check if cookie exists before accessing its value
  if (!orgSessionCookie) {
    redirect("/select-org");
  }

  let organizationId: string | number | undefined;
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

  const params = await searchParams;
  const fromProject =
    typeof params.fromProject === "string" ? params.fromProject : "";

  // Find the initial selected project or use the first one
  const initialSelectedProject = fromProject
    ? projects.find((p) => String(p.id) === fromProject) || projects[0]
    : projects[0];

  return (
    <OrganizationManagmentClient members={members} initialProjects={projects} />
  );
};

export default OrganizationManagementPage;
