"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";

import CreateProject from "./CreateProjectsWidget";
import { sampleProjectData } from "@/app/(home)/dummy_data/data";
import { ProjectsList } from "@/app/(home)/types/types";
import { ExpandableTable } from "../../components/Accordion";
import ExpandedProjectCard from "../../components/ExpandedProjectCard";

const Projects = () => {
  const router = useRouter();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [tableData] = useState<ProjectsList[]>(sampleProjectData);

  const handleExplore = (project: ProjectsList) => {
    router.push(`/project/${project.id}`);
  };

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
        <h1 className="text-2xl font-bold text-secondary-content">
          Welcome Back Kevin
        </h1>
      </div>
      <div className="divider" />

      {/* Main Content */}
      <div className="flex w-full">
        {/* Projects Table */}
        <div className="w-full md:w-2/3 px-4">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-secondary-content text-lg font-semibold">
              Your Projects
            </h3>

            <div className="flex gap-2">
              <button className="btn btn-outline btn-secondary flex items-center gap-1">
                <PlusIcon />
                <span>Record</span>
              </button>
              <button
                onClick={() => setIsModalOpen(true)}
                className="btn btn-secondary text-primary-content flex items-center gap-1"
              >
                <PlusIcon />
                <span>Project</span>
              </button>
            </div>
          </div>

          <ExpandableTable
            data={tableData}
            columns={columns}
            onExplore={handleExplore}
            renderExpandedContent={(project, onClose) => (
              <ExpandedProjectCard project={project} onClose={onClose} />
            )}
          />
        </div>

        {/* Sidebar Placeholder */}
        <div className="md:block w-1/3 px-4">Autumn's Widgets go here 👇</div>
      </div>

      {/* Create Project Modal */}
      <CreateProject
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
      />
    </div>
  );
};

const PlusIcon = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="none"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="currentColor"
    className="size-5"
  >
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M12 4.5v15m7.5-7.5h-15"
    />
  </svg>
);

export default Projects;
