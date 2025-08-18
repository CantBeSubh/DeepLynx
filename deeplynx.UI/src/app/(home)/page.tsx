// app/(home)/page.tsx
import HomeDashboard from "./HomeDashboard";
import { getAllProjects } from "../lib/projects_services";

export default async function Page() {
  const projects = await getAllProjects(); // suspends here
  await new Promise((r) => setTimeout(r, 1200));
  return <HomeDashboard initialProjects={projects} />;
}
