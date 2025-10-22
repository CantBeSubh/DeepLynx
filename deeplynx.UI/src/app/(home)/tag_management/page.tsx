import React from "react";
import TagManagementClient from "./TagManagementClient";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { ProjectResponseDto } from "../types/responseDTOs";

const TagManagementPage = async ({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) => {
  const params = await searchParams;
  const fromProject =
    typeof params.fromProject === "string" ? params.fromProject : "";

  // Keep SSR for projects (fast initial render, no client flash)
  const projects = (await getAllProjectsServer()) as ProjectResponseDto[];
  const initialProjects = projects.map((p) => ({
    id: String(p.id),
    name: p.name,
  }));

  // Let the client fetch records after mount based on the dropdown selection
  const initialSelectedProjects = fromProject ? [fromProject] : [];

  return (
    <TagManagementClient
      initialProjects={initialProjects}
      initialSelectedProjects={initialSelectedProjects}
    />
  );
};

export default TagManagementPage;
