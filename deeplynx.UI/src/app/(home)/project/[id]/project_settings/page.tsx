import React from "react";
import { FileViewerTableRow, ProjectsList } from "../../../types/types";
import ProjectSettingsClient from "./ProjectSettingsClient";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import ProjectSettings from "../../../components/ProjectSettingsTable/ProjectSettings";
import { notFound } from "next/navigation";
import { ProjectDTO } from "@/app/(home)/types/responseDTOs/projectResponseDto";

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

export default async function Page({ params }: Props) {
  const { id } = await params;
  if (!id) return notFound();

  const projectDTOs = (await getAllProjectsServer()) as ProjectDTO[];
  const initialProjects = projectDTOs.map((p) => toProjectDTOs(p));
  const initialProject = initialProjects.find((p) => p.id == id);

  if (initialProject == undefined) return notFound();

  return (
    <ProjectSettings
      projects={initialProjects}
      initialProject={initialProject}
    />
  );
}