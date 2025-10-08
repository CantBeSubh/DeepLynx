// app/(home)/(routes)/data_catalog/page.tsx
import { ProjectDTO } from "../types/responseDTOs/projectResponseDto";
import { FileViewerTableRow } from "../types/types";
import DataCatalogClient from "./DataCatalogClient";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";


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

  return (
    <DataCatalogClient
      initialProjects={initialProjects}
      initialSelectedProjects={initialSelectedProjects}
      initialSearchTerm={initialSearch}
      initialRecords={initialRecords}
    />
  );
}
