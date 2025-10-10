import React from "react";
import { FileViewerTableRow } from "../types/types";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import MemberManagementClient from "./MemberManagementClient";
import { ProjectResponseDto } from "../types/responseDTOs";


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
  const projects = (await getAllProjectsServer()) as ProjectResponseDto[];
  const initialProjects = projects.map((p) => ({
    id: String(p.id),
    name: p.name,
  }));

  // Let the client fetch records after mount based on the dropdown selection
  const initialSelectedProjects = fromProject ? [fromProject] : [];
  const initialRecords = [] as FileViewerTableRow[];

  return (
    <MemberManagementClient
    />
  );
}