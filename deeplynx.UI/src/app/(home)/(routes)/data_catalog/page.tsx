// app/(home)/(routes)/data_catalog/page.tsx
import DataCatalogClient from "./DataCatalogClient";
import {
  getAllProjectsServer,
  getAllRecordsForMultipleProjectsServer,
} from "@/app/lib/projects_services.server";
import { FileViewerTableRow } from "@/app/(home)/types/types";

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

  // Type the service results (or change the service signatures to return typed data)
  const projects = (await getAllProjectsServer()) as ProjectDTO[];
  const initialProjects: { id: string; name: string }[] = projects.map((p) => ({
    id: String(p.id),
    name: p.name,
  }));

  const initialSelectedProjects = fromProject ? [fromProject] : [];

  let initialRecords: FileViewerTableRow[] = [];
  if (initialSelectedProjects.length) {
    const idsNum = initialSelectedProjects.map((id) => Number(id));
    initialRecords = (await getAllRecordsForMultipleProjectsServer(
      idsNum
    )) as FileViewerTableRow[];
  }

  // demo delay
  await new Promise((r) => setTimeout(r, 1200));

  return (
    <DataCatalogClient
      initialProjects={initialProjects}
      initialSelectedProjects={initialSelectedProjects}
      initialSearchTerm={initialSearch}
      initialRecords={initialRecords}
    />
  );
}
