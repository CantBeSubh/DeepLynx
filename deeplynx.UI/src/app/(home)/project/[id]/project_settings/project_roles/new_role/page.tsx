import React from "react";
import { getAllProjectsServer } from "@/app/lib/projects_services.server";
import { notFound } from "next/navigation";

type ProjectDTO = { id: number | string; name: string };

export default async function Page({
  params,
  searchParams,
}: {
  params: Promise<{ id: number }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const { id } = await params;
  const search = await searchParams;

  if (!id) return notFound();

  const roleId = typeof search.roleId === "string" ? search.roleId : "";
  const fromProject =
    typeof search.fromProject === "string" ? search.fromProject : "";
  const initialSearch = typeof search.search === "string" ? search.search : "";

  return <div>Place holder</div>;
}
