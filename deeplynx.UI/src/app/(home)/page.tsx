// app/(home)/page.tsx
import HomeDashboard from "./HomeDashboard";
import { getAllProjectsServer } from "../lib/projects_services";

export default async function Page() {
  const projects = await getAllProjectsServer(); // suspends here
  await new Promise((r) => setTimeout(r, 1200));
  return <HomeDashboard initialProjects={projects} />;
}
