// app/(home)/(routes)/project/[id]/page.tsx
import { notFound } from "next/navigation";
import ProjectDetailClient from "./ProjectDetailClient";
import { getProject } from "@/app/lib/projects_services";

type Props = {
  // In async pages, Next may pass a Promise for params
  params: Promise<{ id?: string }>;
};

export default async function ProjectPage({ params }: Props) {
  const p = await params;
  const id = p.id;
  if (!id) return notFound();

  // server fetch (no axios to /api)
  const project = await getProject(id);
  if (!project) return notFound();
  await new Promise((r) => setTimeout(r, 1200));
  return <ProjectDetailClient initialProject={project} projectId={id} />;
}
