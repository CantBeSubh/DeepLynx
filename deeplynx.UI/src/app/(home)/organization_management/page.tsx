// src/app/(home)/organization_management/page.tsx

import { getAllGroups } from "@/app/lib/group_services.server";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { getAllUsersServer } from "@/app/lib/user_services.server";
import { getAllRolesServer } from "@/app/lib/role_services.server"; // Add this import
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { mapToProjectResponseDtos } from "../page";
import {
  GroupResponseDto,
  ProjectResponseDto,
  UserResponseDto,
  RoleResponseDto,
  PermissionResponseDto,
} from "../types/responseDTOs";
import OrganizationManagmentClient from "./OrganizationManagementClient";
import { getAllPermissionsServer } from "@/app/lib/permissions_services.server";

export const dynamic = "force-dynamic";

const mapToGroupResponseDtos = (group: GroupResponseDto): GroupResponseDto => {
  return {
    ...group,
  } as GroupResponseDto;
};

const OrganizationManagementPage = async ({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) => {
  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

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

  // Fetch roles filtered by organization
  let roles: RoleResponseDto[] = [];
  try {
    const apiRoles = await getAllRolesServer({
      organizationId: Number(organizationId),
      hideArchived: true,
    });
    roles = apiRoles;
  } catch (error) {
    console.error("getAllRolesServer failed:", error);
  }

  // Fetch permissions filtered by organization
  let permissions: PermissionResponseDto[] = [];
  try {
    const apiPermissions = await getAllPermissionsServer({
      organizationId: Number(organizationId),
      hideArchived: true,
    });
    permissions = apiPermissions;
  } catch (error) {
    console.error("getAllPermissionsServer failed:", error);
  }

  // Fetch users filtered by organization
  const members = (await getAllUsersServer(
    undefined,
    Number(organizationId)
  )) as UserResponseDto[];

  const params = await searchParams;
  const fromProject =
    typeof params.fromProject === "string" ? params.fromProject : "";

  const initialSelectedProject = fromProject
    ? projects.find((p) => String(p.id) === fromProject) || projects[0]
    : projects[0];

  return (
    <OrganizationManagmentClient
      members={members}
      initialProjects={projects}
      initialGroups={groups}
      initialRoles={roles}
      initialSelectedProject={initialSelectedProject}
      initialPermissions={permissions}
    />
  );
};

export default OrganizationManagementPage;
