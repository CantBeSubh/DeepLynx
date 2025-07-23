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

const Projects = () => {
  const router = useRouter();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [widgetModal, setWidgetModal] = useState(false);
  const homeWidgets: WidgetType[] = ["Links", "DataOverview", "Graph"];
  const [projects, setProjects] = useState([]);

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
      <div className="flex justify-between items-center px-4 py-4">
        <h1 className="text-2xl font-bold text-info-content">
          Welcome Back Kevin
        </h1>
      </div>
      <div className="divider" />

      {/* Main Content */}
      <div className="flex w-full">
        <div className="w-full md:w-1/2 px-4">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-info-content text-lg font-semibold">
              Your Projects
            </h3>

            <div className="flex gap-2">
              <button className="btn btn-outline btn-secondary flex items-center gap-1">
                <PlusIcon className="size-6" />
                <span>Record</span>
              </button>
              <button
                onClick={() => setIsModalOpen(true)}
                className="btn btn-secondary text-primary-content flex items-center gap-1"
              >
                <PlusIcon className="size-6" />
                <span>Project</span>
              </button>
            </div>
          </div>

          <ExpandableTable
            data={projects}
            columns={columns}
            onExplore={handleExplore}
            renderExpandedContent={(project, onClose) => (
              <ExpandedProjectCard project={project} onClose={onClose} />
            )}
          />
        </div>

        <div className="w-full md:w-1/2 px-4">
          <div className="flex justify-between items-center justify-end mb-4">
            <button className="btn btn-outline btn-secondary flex items-center mr-2">
              <Cog6ToothIcon className="size-6" />
              Customize
            </button>
            <button
              onClick={() => setWidgetModal(true)}
              className="btn btn-secondary text-primary-content flex items-center"
            >
              <PlusIcon className="size-6" />
              Widget
            </button>
          </div>
          <WidgetCard widgets={homeWidgets} />
        </div>
      </div>

      {/* Create Project Modal */}
      <CreateProject
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
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
