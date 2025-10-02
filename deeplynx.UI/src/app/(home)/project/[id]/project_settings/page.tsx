import React from "react";
import { FileViewerTableRow, ProjectsList } from "../../../types/types";
import ProjectSettingsClient from "./ProjectSettingsClient";
import { getAllProjectsServer, ProjectDTO } from "@/app/lib/projects_services.server";
import ProjectSettings from "../../../components/ProjectSettingsTable/ProjectSettings";
import { notFound } from "next/navigation";

function toProjectsList(p: ProjectDTO): ProjectsList {
  return {
    id: String(p.id), // <- normalize to string
    name: p.name ?? "",
    description: p.description ?? "",
    lastUpdatedAt: p.lastUpdatedAt,
  };
}

type Props = {
  params: Promise<{ id?: string }>;
};

export default async function ProjectPage({ params }: Props) {
  const { id } = await params;
  if (!id) return notFound();

  const projectDTOs = (await getAllProjectsServer()) as ProjectDTO[];
  const initialProjects = projectDTOs.map((p) => toProjectsList(p));
  const initialProject = initialProjects.find((p) => p.id == id);

  if (initialProject == undefined) return notFound();

  return (
    <ProjectSettings
      projects={initialProjects}
      initialProject={initialProject}
    />
  );
}