import React from "react";
import { FileViewerTableRow } from "../../../../types/types";
import RoleSettingsClient from "./RoleSettingsClient";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { notFound } from "next/navigation";

type ProjectDTO = { id: number | string; name: string };

export default async function Page({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const { id } = await params;  // Get id from params, not searchParams
  const search = await searchParams;

  if (!id) return notFound();

  const roleId = typeof search.roleId === "string" ? search.roleId : "";
  const fromProject = typeof search.fromProject === "string" ? search.fromProject : "";
  const initialSearch = typeof search.search === "string" ? search.search : "";

  // Rest of your code...

  return (
    <RoleSettingsClient
      projectId={id}
    />
  )
}