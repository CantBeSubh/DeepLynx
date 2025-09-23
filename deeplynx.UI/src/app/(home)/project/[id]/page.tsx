// app/(home)/(routes)/project/[id]/page.tsx
import { notFound } from "next/navigation";
import ProjectDetailClient from "./ProjectDetailClient";
import {
  getProjectServer,
  type ProjectDTO,
} from "@/app/lib/projects_services.server";
import type { ProjectsList } from "@/app/(home)/types/types";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
function toProjectsList(p: ProjectDTO): ProjectsList {
  return {
    id: String(p.id), // <- normalize to string
    name: p.name ?? "",
    description: p.description ?? "",
    lastViewed: p.lastViewed ?? "",
    createdAt: p.createdAt ?? "",
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
    <ProjectDetailClient
      projects={initialProjects}
      initialProject={initialProject}
      projectId={id}
    />
  );
}
