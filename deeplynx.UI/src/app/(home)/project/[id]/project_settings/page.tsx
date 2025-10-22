import React from "react";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import ProjectSettingsClient from "./ProjectSettingsClient";
import { notFound } from "next/navigation";
import { ProjectResponseDto } from "@/app/(home)/types/responseDTOs";
function toProjectResponseDtos(p: ProjectResponseDto): ProjectResponseDto {
  return {
    id: String(p.id),
    name: p.name ?? "",
    description: p.description ?? "", // fallback to empty string
    abbreviation: p.abbreviation ?? "",
    lastUpdatedAt: p.lastUpdatedAt,
    lastUpdatedBy: p.lastUpdatedBy ?? "",
    isArchived: p.isArchived,
    organizationId: p.organizationId
  };
}

type Props = {
  params: Promise<{ id?: string }>;
};

export default async function Page({ params }: Props) {
  const { id } = await params;
  if (!id) return notFound();

  const ProjectResponseDtos = (await getAllProjectsServer()) as ProjectResponseDto[];
  const initialProjects = ProjectResponseDtos.map((p) => toProjectResponseDtos(p));
  const initialProject = initialProjects.find((p) => p.id == id);

  if (initialProject == undefined) return notFound();

  return (
    <ProjectSettingsClient
      projects={initialProjects}
      initialProject={initialProject}
    />
  );
}