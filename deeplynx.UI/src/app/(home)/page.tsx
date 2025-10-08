// app/(home)/page.tsx
import HomeDashboardClient from "./HomeDashboardClient";
import { getAllProjectsServer} from "../lib/projects_services.server";
import { ProjectDTO } from "./types/responseDTOs/projectResponseDto";
export const dynamic = "force-dynamic"; // if behind auth

function mapToProjectDTOs(p: ProjectDTO): ProjectDTO {
  return {
  id: String(p.id),
  name: p.name ?? "",
  description: p.description ?? "", // fallback to empty string
  abbreviation:p.abbreviation ?? "",
  lastUpdatedAt: p.lastUpdatedAt,
  lastUpdatedBy: p.lastUpdatedBy ?? "",
  isArchived: p.isArchived,
  organizationId: p.organizationId
};
}

export default async function Page() {
  let projects: ProjectDTO[] = [];
  try {
    const apiProjects = await getAllProjectsServer();
    projects = apiProjects.map(mapToProjectDTOs);
  } catch (e) {
    console.error("getAllProjectsServer failed:", e);
  }

  // Local development bypass
  const disableAuth = process.env.DISABLE_FRONTEND_AUTHENTICATION;

  if (disableAuth == "true") {
    console.log(disableAuth)
    return <HomeDashboardClient initialProjects={projects} />;
  } else

    return (
      // <AuthGuard>
      <HomeDashboardClient initialProjects={projects} />
      // </AuthGuard>
    );
}