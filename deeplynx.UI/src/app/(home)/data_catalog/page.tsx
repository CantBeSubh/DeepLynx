// app/(home)/(routes)/data_catalog/page.tsx
import { FileViewerTableRow } from "../types/types";
import DataCatalogClient from "./DataCatalogClient";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";

type ProjectDTO = { id: number | string; name: string };

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const params = await searchParams;
  const fromProject =
    typeof params.fromProject === "string" ? params.fromProject : "";
  const initialSearch = typeof params.search === "string" ? params.search : "";

  // Keep SSR for projects (fast initial render, no client flash)
  const projects = (await getAllProjectsServer()) as ProjectDTO[];
  const initialProjects = projects.map((p) => ({
    id: String(p.id),
    name: p.name,
  }));

  // Let the client fetch records after mount based on the dropdown selection
  const initialSelectedProjects = fromProject ? [fromProject] : [];
  const initialRecords = [] as FileViewerTableRow[];
//helper function for delay
//  const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

//   // 👇 artificial delay (e.g., 2 seconds)
//   await delay(2000);
  return (
    <DataCatalogClient
      initialProjects={initialProjects}
      initialSelectedProjects={initialSelectedProjects}
      initialSearchTerm={initialSearch}
      initialRecords={initialRecords}
    />
  );
}
