// app/(home)/(routes)/data_catalog/page.tsx
import DataCatalogClient from "./DataCatalogClient";
import {
  getAllProjects,
  getAllRecordsForMultipleProjects,
} from "@/app/lib/projects_services"; // server-only versions (no axios to /api)

type PageProps = {
  searchParams: {
    fromProject?: string;
    search?: string;
  };
};

export default async function Page({ searchParams }: PageProps) {
  const params = await searchParams;
  const fromProject = params.fromProject ?? "";
  const initialSearch = params.search ?? "";

  // 1) Load projects on the server
  const projects = await getAllProjects(); // returns [{id, name, ...}]
  const initialProjects = projects.map((p: any) => ({
    id: String(p.id),
    name: p.name as string,
  }));

  // 2) If a project is preselected, preload its records (optional)
  const initialSelectedProjects = fromProject ? [fromProject] : [];
  let initialRecords: any[] = [];
  if (initialSelectedProjects.length > 0) {
    const idsNum = initialSelectedProjects.map((id) => Number(id));
    initialRecords = await getAllRecordsForMultipleProjects(idsNum);
  }
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
