import React from "react";
import TagManagementClient from "./TagManagementClient";
import { getAllProjectsServer } from "@/app/lib/server_service/projects_services.server";
import { ProjectResponseDto } from "../../types/responseDTOs";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";

const TagManagementPage = async ({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) => {
  const params = await searchParams;
  const fromProject =
    typeof params.fromProject === "string" ? params.fromProject : "";
  // Get organization from cookies
  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

  // If no org selected, redirect to selection page
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
  // Keep SSR for projects (fast initial render, no client flash)
  const projects = (await getAllProjectsServer(
    organizationId as number
  )) as ProjectResponseDto[];

  // Find the initial selected project or use the first one
  const initialSelectedProject = fromProject
    ? projects.find((p) => String(p.id) === fromProject) || projects[0]
    : projects[0];

  return (
    <TagManagementClient
      initialProjects={projects}
      initialSelectedProject={initialSelectedProject}
    />
  );
};

export default TagManagementPage;
