// app/(home)/ProjectsClient.tsx  — Client Component
"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link"; // prefer Link over useRouter().push
import CreateProject from "./components/CreateProjectsModal";
import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import { ProjectsList } from "@/app/(home)/types/types";
import { ExpandableTable } from "@/app/(home)/components/ExpandableTable";
import ExpandedProjectCard from "@/app/(home)/components/ExpandedProjectCard";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { PlusIcon, Cog6ToothIcon } from "@heroicons/react/24/outline";
import SearchInput from "./components/SearchInput";
import { translations } from "../lib/translations";
import { getAllProjects } from "../lib/projects_services"; // optional (for refresh)
import { useRouter } from "next/navigation";

type Props = { initialProjects: ProjectsList[] };

export default function HomeDashboard({ initialProjects }: Props) {
  const locale = "en";
  const t = translations[locale];
  const router = useRouter();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [widgetModal, setWidgetModal] = useState(false);
  const [projects, setProjects] = useState<ProjectsList[]>(initialProjects);
  const [searchTerm, setSearchTerm] = useState("");
  const homeWidgets: WidgetType[] = ["Links", "DataOverview", "Graph"];

  const filteredProjects = projects.filter((project) => {
    const term = searchTerm.toLowerCase();
    return (
      project.name.toLowerCase().includes(term) ||
      project.description?.toLowerCase().includes(term)
    );
  });

  const refreshProjects = async () => {
    try {
      const data = await getAllProjects();
      setProjects(data);
    } catch (err) {
      console.error("Failed to refresh projects:", err);
    }
  };

  const onExplore = (row: ProjectsList) => {
    router.push(`/project/${row.id}`);
  };

  const columns = [
    {
      header: "Project Name",
      data: (row: ProjectsList) => (
        <Link href={`/project/${row.id}`} className="font-bold underline">
          {row.name}
        </Link>
      ),
    },
    { header: "Description", data: (row: ProjectsList) => row.description },
    { header: "Last Viewed", data: (row: ProjectsList) => row.lastViewed },
  ];

  return (
    <div className="bg-base-100">
      {/* Header */}
      <div className="flex justify-between items-center bg-base-200/40 pl-12 pt-3 pb-2">
        <h1 className="text-2xl font-bold text-info-content">
          {t.translations.WELECOME}
        </h1>
        <SearchInput
          placeholder="Search Projects"
          onChange={(e) => setSearchTerm(e.target.value)}
          className="mr-6"
        />
      </div>

      {/* Main Content */}
      <div className="mr-6 py-6">
        <div className="flex justify-between items-center justify-end mb-4">
          <button className="btn btn-outline btn-secondary flex items-center mr-2">
            <Cog6ToothIcon className="size-6" />
            {t.translations.CUSTOMIZE}
          </button>
          <button
            onClick={() => setWidgetModal(true)}
            className="btn btn-secondary text-primary-content flex items-center"
          >
            <PlusIcon className="size-6" />
            {t.translations.WIDGET}
          </button>
        </div>
        <div className="flex">
          <div className="w-full md:w-1/2 px-4">
            <div className="card card-border p-4">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-info-content text-lg font-semibold">
                  {t.translations.YOUR_PROJECTS}
                </h3>

                <div className="flex gap-2">
                  <button className="btn btn-outline btn-secondary flex items-center gap-1">
                    <PlusIcon className="size-6" />
                    <span>{t.translations.RECORD}</span>
                  </button>
                  <button
                    onClick={() => setIsModalOpen(true)}
                    className="btn btn-secondary text-primary-content flex items-center gap-1"
                  >
                    <PlusIcon className="size-6" />
                    <span>{t.translations.PROJECT}</span>
                  </button>
                </div>
              </div>

              <ExpandableTable
                data={filteredProjects}
                columns={columns}
                // If your table needs an "Explore" button, use Link inside column renderers
                renderExpandedContent={(project, onClose) => (
                  <ExpandedProjectCard project={project} onClose={onClose} />
                )}
                onExplore={onExplore}
              />
            </div>
          </div>
          <div className="w-full md:w-1/2">
            <WidgetCard widgets={homeWidgets} />
          </div>
        </div>
      </div>

      {/* Modals */}
      <CreateProject
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onProjectCreated={refreshProjects}
      />
      <CreateWidget
        isOpen={widgetModal}
        onClose={() => setWidgetModal(false)}
      />
    </div>
  );
}
