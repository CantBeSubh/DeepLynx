// app/(home)/(routes)/project/[id]/page.tsx
import { notFound } from "next/navigation";
import ProjectDetailClient from "./ProjectDetailClient";
import {
  getProjectServer,
  type ProjectDTO,
} from "@/app/lib/projects_services.server";
import type { ProjectsList } from "@/app/(home)/types/types";

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

  const dto = await getProjectServer(id);
  if (!dto) return notFound();

  const project: ProjectsList = toProjectsList(dto);

  await new Promise((r) => setTimeout(r, 1200));

  return <ProjectDetailClient initialProject={project} projectId={id} />;
}
