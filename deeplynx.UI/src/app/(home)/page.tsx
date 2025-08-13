"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { getAllProjects } from "../lib/projects_services";

import CreateProject from "./components/CreateProjectsModal";
import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import { ProjectsList } from "@/app/(home)/types/types";
import { ExpandableTable } from "@/app/(home)/components/ExpandableTable";
import ExpandedProjectCard from "@/app/(home)/components/ExpandedProjectCard";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { PlusIcon, Cog6ToothIcon } from "@heroicons/react/24/outline";
import SearchInput from "./components/SearchInput";
import { translations } from "../lib/translations";

const Projects = () => {
  const router = useRouter();
  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [widgetModal, setWidgetModal] = useState(false);
  const homeWidgets: WidgetType[] = ["Links", "DataOverview", "Graph"];
  const [projects, setProjects] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");

  const filteredProjects = projects.filter((project: ProjectsList) => {
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
    } catch (error) {
      console.error("Failed to refresh projects:", error);
    }
  };

  const handleExplore = (project: ProjectsList) => {
    router.push(`/project/${project.id}`);
  };

  useEffect(() => {
    getAllProjects().then(setProjects).catch(console.error);
  }, []);

  const columns = [
    {
      header: "Project Name",
      data: (row: ProjectsList) => (
        <span className="font-bold">{row.name}</span>
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
          {t.HomeDashboard.WELECOME}
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
            {t.HomeDashboard.CUSTOMIZE}
          </button>
          <button
            onClick={() => setWidgetModal(true)}
            className="btn btn-secondary text-primary-content flex items-center"
          >
            <PlusIcon className="size-6" />
            {t.HomeDashboard.WIDGET}
          </button>
        </div>
        <div className="flex">
          <div className="w-full md:w-1/2 px-4">
            <div className="card card-border p-4">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-info-content text-lg font-semibold">
                  {t.HomeDashboard.YOUR_PROJECTS}
                </h3>

                <div className="flex gap-2">
                  <button className="btn btn-outline btn-secondary flex items-center gap-1">
                    <PlusIcon className="size-6" />
                    <span>{t.HomeDashboard.RECORD}</span>
                  </button>
                  <button
                    onClick={() => setIsModalOpen(true)}
                    className="btn btn-secondary text-primary-content flex items-center gap-1"
                  >
                    <PlusIcon className="size-6" />
                    <span>{t.HomeDashboard.PROJECT}</span>
                  </button>
                </div>
              </div>
              <ExpandableTable
                data={filteredProjects}
                columns={columns}
                onExplore={handleExplore}
                renderExpandedContent={(project, onClose) => (
                  <ExpandedProjectCard project={project} onClose={onClose} />
                )}
              />
            </div>
          </div>
          <div className="w-full md:w-1/2">
            <WidgetCard widgets={homeWidgets} />
          </div>
        </div>
      </div>

      {/* Create Project Modal */}
      <CreateProject
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onProjectCreated={refreshProjects}
      />

      {/* Create Widget Modal */}
      <CreateWidget
        isOpen={widgetModal}
        onClose={() => setWidgetModal(false)}
      />
    </div>
  );
};

export default Projects;
