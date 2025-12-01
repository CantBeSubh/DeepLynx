// src/app/(home)/project_management/[id]/page.tsx

import React from "react";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import ProjectManagementClient from "./ProjectManagementClient";
import {
  GroupResponseDto,
  ProjectResponseDto,
  RoleResponseDto,
  PermissionResponseDto,
  ProjectMemberResponseDto,
} from "@/app/(home)/types/responseDTOs";
import {
  getProjectServer,
  getProjectMembersServer,
} from "@/app/lib/server_service/projects_services.server";
import { getAllRolesServer } from "@/app/lib/server_service/role_services.server";
import { getPermissionsForRoleServer } from "@/app/lib/server_service/permissions_services.server";

export const dynamic = "force-dynamic";

interface PageProps {
  params: {
    id: string;
  };
}

const Page = async ({ params }: PageProps) => {
  // Redirect if projectId is missing or invalid
  if (!params?.id || isNaN(Number(params.id))) {
    redirect("/");
  }

  const projectId = Number(params.id);

  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

  if (!orgSessionCookie) {
    redirect("/select-org");
  }

  let organizationId: number;
  try {
    const orgSession = JSON.parse(orgSessionCookie.value);
    organizationId = Number(orgSession.organizationId);
  } catch (e) {
    console.error("Failed to parse organization session:", e);
    redirect("/select-org");
  }

  let project: ProjectResponseDto | null = null;
  let projectMembers: ProjectMemberResponseDto[] = [];
  let projectRoles: RoleResponseDto[] = [];
  let projectPermissions: PermissionResponseDto[] = [];

  if (!isNaN(organizationId) && !isNaN(projectId)) {
    try {
      project = await getProjectServer(organizationId, projectId, true);
    } catch (e) {
      console.error("getProjectServer failed:", e);
    }

    try {
      projectMembers = await getProjectMembersServer(organizationId, projectId);
    } catch (e) {
      console.error("getProjectMembersServer failed:", e);
    }

    try {
      projectRoles = await getAllRolesServer(organizationId, projectId);
    } catch (e) {
      console.error("getAllRoles failed: ", e);
    }

    try {
      if (projectRoles.length > 0) {
        const permsByRole: PermissionResponseDto[][] = await Promise.all(
          projectRoles.map((role) =>
            getPermissionsForRoleServer(
              organizationId,
              projectId,
              role.id
            ).catch((error) => {
              console.error(
                `getPermissionsForRoleServer failed for role ${role.id}:`,
                error
              );
              return [] as PermissionResponseDto[];
            })
          )
        );

        // Flatten and dedupe by permission id
        const permsMap = new Map<number | string, PermissionResponseDto>();

        permsByRole.flat().forEach((perm: PermissionResponseDto) => {
          const key = perm.id;
          if (!permsMap.has(key)) {
            permsMap.set(key, perm);
          }
        });

        projectPermissions = Array.from(permsMap.values());
      }
    } catch (e) {
      console.error("Aggregating permissions by role failed: ", e);
    }

    // TODO: later: fetch real groups for this project
    const projectGroups: GroupResponseDto[] = [];

    return (
      <ProjectManagementClient
        project={project}
        projectMembers={projectMembers}
        projectGroups={projectGroups}
        projectRoles={projectRoles}
        projectPermissions={projectPermissions}
      />
    );
  }
};

export default Page;
