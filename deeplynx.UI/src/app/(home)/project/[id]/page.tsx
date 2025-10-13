// app/(home)/(routes)/project/[id]/page.tsx
import { notFound } from "next/navigation";
import ProjectDetailClient from "./ProjectDetailClient";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { ProjectResponseDto } from "../../types/responseDTOs";

function toProjectResponseDtos(p: ProjectResponseDto): ProjectResponseDto {
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

  const ProjectResponseDtos = (await getAllProjectsServer()) as ProjectResponseDto[];
  console.log(ProjectResponseDtos);
  const initialProjects = ProjectResponseDtos.map((p) => toProjectResponseDtos(p));
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
