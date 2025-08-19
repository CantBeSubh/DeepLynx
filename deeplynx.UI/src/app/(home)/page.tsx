// app/(home)/page.tsx
import HomeDashboard from "./HomeDashboardClient";
import {
  getAllProjectsServer,
  type ProjectDTO, // make sure this includes optional fields below
} from "../lib/projects_services.server";
import type { ProjectsList } from "./types/types";

export const dynamic = "force-dynamic"; // if behind auth

function mapToProjectsList(p: ProjectDTO): ProjectsList {
  return {
    id: String(p.id),
    name: p.name ?? "",
    description: p.description ?? "", // fallback to empty string
    lastViewed: p.lastViewed ?? "", // or a formatted date if you have it
    createdAt: p.createdAt ?? "", // note: your type had "createdAtt" in the error; use the real key
  };
}

export default async function Page() {
  let projects: ProjectsList[] = [];
  try {
    const apiProjects = await getAllProjectsServer(); // ProjectDTO[]
    projects = apiProjects.map(mapToProjectsList);
  } catch (e) {
    console.error("getAllProjectsServer failed:", e);
  }

  return <HomeDashboard initialProjects={projects} />;
}
