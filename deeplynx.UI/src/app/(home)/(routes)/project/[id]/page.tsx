// app/(home)/project/[id]/page.tsx
import ProjectDetailClient from "./ProjectDetailClient";
import { getProject } from "@/app/lib/projects_services"; // server-only

type Props = {
  params: { id: string };
};

export default async function ProjectPage({ params }: Props) {
  await new Promise((r) => setTimeout(r, 1200));
  const project = await getProject(params.id); // server fetch (no axios to /api)
  // If you prefer, handle null here and render a simple not-found UI.
  return <ProjectDetailClient initialProject={project} projectId={params.id} />;
}
