// app/(home)/page.tsx
import HomeDashboardClient from "./HomeDashboardClient";
import { getAllProjectsServer } from "../lib/server_service/projects_services.server";
import { ProjectResponseDto } from "./types/responseDTOs";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";

export const dynamic = "force-dynamic";

export function mapToProjectResponseDtos(
  p: ProjectResponseDto
): ProjectResponseDto {
  return {
    id: String(p.id),
    name: p.name ?? "",
    description: p.description ?? "",
    abbreviation: p.abbreviation ?? "",
    lastUpdatedAt: p.lastUpdatedAt,
    lastUpdatedBy: p.lastUpdatedBy ?? "",
    isArchived: p.isArchived,
    organizationId: p.organizationId,
  };
}

export default async function Page() {
  const isAuthDisabled = 
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  // Get organization from cookies
  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

  // If no org selected, redirect to selection page (unless auth is disabled)
  if (!orgSessionCookie && !isAuthDisabled) {
    redirect("/select-org");
  }

  let organizationId: string | number | undefined;
  
  if (orgSessionCookie) {
    try {
      const orgSession = JSON.parse(orgSessionCookie.value);
      organizationId = orgSession.organizationId;
    } catch (e) {
      console.error("Failed to parse organization session:", e);
      if (!isAuthDisabled) {
        redirect("/select-org");
      }
    }
  }

  // When auth is disabled and no org cookie, use a default or skip org filtering
  if (isAuthDisabled && !organizationId) {
    // Option 1: Fetch all projects without org filter
    // Option 2: Use a default org ID
    // For now, let's fetch all projects
    organizationId = undefined; // or set a default: organizationId = 1;
  }

  // Fetch projects filtered by organization
  let projects: ProjectResponseDto[] = [];
  try {
    const apiProjects = await getAllProjectsServer(
      organizationId as number,
      true
    );
    projects = apiProjects.map(mapToProjectResponseDtos);
  } catch (e) {
    console.error("getAllProjectsServer failed:", e);
  }

  return <HomeDashboardClient initialProjects={projects} />;
}