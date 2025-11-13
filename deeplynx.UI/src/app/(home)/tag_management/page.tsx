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

  // Find the initial selected project or use the first one
  const initialSelectedProject = fromProject
    ? projects.find((p) => String(p.id) === fromProject) || projects[0]
    : projects[0];

  return (
    <TagManagementClient
      initialProjects={projects}
      initialSelectedProject={initialSelectedProject}
    />
  );
};

export default TagManagementPage;
