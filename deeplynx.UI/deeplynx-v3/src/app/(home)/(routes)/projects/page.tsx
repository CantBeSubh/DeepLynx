"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import CreateProject from "./CreateProjectsWidget";
import { sampleProjectData } from "@/app/(home)/dummy_data/data";
import { ProjectsList } from "@/app/(home)/types/types";
import { ExpandableTable } from "../../components/Accordion";

const Projects = () => {
  const router = useRouter();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [tableData, setTableData] = useState<ProjectsList[]>(sampleProjectData);

  const openModal = () => setIsModalOpen(true);
  const closeModal = () => setIsModalOpen(false);

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
      {/* Page Title */}
      <div className="flex justify-between items-center px-4 py-4">
        <h1 className="text-2xl font-bold text-secondary-content">
          Welcome Back Kevin
        </h1>
      </div>
      <div className="divider" />

      {/* Projects Section */}
      <div className="flex w-full">
        <div className="w-full md:w-2/3 px-4">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-secondary-content text-lg font-semibold">
              Your Projects
            </h3>
            <button onClick={openModal}>
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                fill="currentColor"
                className="size-10 text-secondary cursor-pointer"
              >
                <path
                  fillRule="evenodd"
                  d="M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25ZM12.75 9a.75.75 0 0 0-1.5 0v2.25H9a.75.75 0 0 0 0 1.5h2.25V15a.75.75 0 0 0 1.5 0v-2.25H15a.75.75 0 0 0 0-1.5h-2.25V9Z"
                  clipRule="evenodd"
                />
              </svg>
            </button>
          </div>

          <ExpandableTable
            data={tableData}
            columns={columns}
            onExplore={handleExplore}
            renderExpandedContent={(project, onClose) => (
              <>
                <div className="flex justify-between items-start">
                  <div>
                    <h2 className="text-2xl font-bold">{project.name}</h2>
                    <p className="text-sm text-base-content">
                      {project.description}
                    </p>
                    <p className="text-sm text-base-300 mt-1 mb-2">
                      Last Edited: {project.lastViewed}
                    </p>
                  </div>
                  <button onClick={onClose} aria-label="Close details">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                      strokeWidth={1.5}
                      stroke="currentColor"
                      className="size-6"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="m4.5 15.75 7.5-7.5 7.5 7.5"
                      />
                    </svg>
                  </button>
                </div>

                <div className="space-x-2">
                  <p className="text-base-300 mb-2">Team Members:</p>
                  {[...Array(5)].map((_, i) => (
                    <div key={i} className="avatar inline-block">
                      <div className="w-10 rounded-full">
                        <img
                          src={`https://i.pravatar.cc/150?img=${i + 1}`}
                          alt="team member avatar"
                        />
                      </div>
                    </div>
                  ))}
                </div>

                <div className="grid grid-cols-3 gap-4 mt-4">
                  {[
                    {
                      title: "Classes",
                      value: 25,
                      iconPath: "M4.098 19.902...",
                    },
                    {
                      title: "Data Records",
                      value: 30,
                      iconPath: "M20.25 6.375...",
                    },
                    {
                      title: "Connections",
                      value: 1250,
                      iconPath: "M7.5 21...",
                    },
                  ].map(({ title, value, iconPath }, idx) => (
                    <div key={idx} className="stat flex items-center">
                      <div>
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          fill="none"
                          viewBox="0 0 24 24"
                          strokeWidth={1.5}
                          stroke="currentColor"
                          className="size-8 text-secondary"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d={iconPath}
                          />
                        </svg>
                      </div>
                      <div>
                        <div className="stat-title text-secondary">{title}</div>
                        <div className="stat-value text-secondary">{value}</div>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="flex justify-end">
                  <button className="btn btn-secondary text-primary-content mt-4">
                    Explore
                  </button>
                </div>
              </>
            )}
          />
        </div>

        <div className="md:block w-1/3 px-4">Autumn's Widgets go here 👇</div>
      </div>

      <CreateProject isOpen={isModalOpen} onClose={closeModal} />
    </div>
  );
};

export default Projects;
