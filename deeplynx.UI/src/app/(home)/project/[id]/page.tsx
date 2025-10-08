// app/(home)/(routes)/project/[id]/page.tsx
import { notFound } from "next/navigation";
import ProjectDetailClient from "./ProjectDetailClient";
import type { ProjectsList } from "@/app/(home)/types/types";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { ProjectDTO } from "../../types/responseDTOs/projectResponseDto";

function toProjectDTOs(p: ProjectDTO): ProjectDTO {
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

type Props = {
  params: Promise<{ id?: string }>;
};

export default async function ProjectPage({ params }: Props) {
  const { id } = await params;
  if (!id) return notFound();

  const projectDTOs = (await getAllProjectsServer()) as ProjectDTO[];
  console.log(projectDTOs);
  const initialProjects = projectDTOs.map((p) => toProjectDTOs(p));
  const initialProject = initialProjects.find((p) => p.id == id);

  if (initialProject == undefined) return notFound();

  return (
    <ProjectDetailClient
      projects={initialProjects}
      initialProject={initialProject}
      projectId={id}
    />
  );
}
