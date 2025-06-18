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
        <div className="w-full md:w-1/2 px-4">
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
        <div className="w-full md:w-1/2 px-4">
          <div className="my-4 flex justify-between items-center justify-end">
            <button
              className="btn btn-outline btn-sm btn-accent mr-2"
              // onClick={addWidget} // Adds new widget when clicked
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z" />
              <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
              </svg>
              Customize
            </button>
            <button
              className="btn btn-outline btn-sm btn-accent"
              // onClick={addWidget} // Adds new widget when clicked
            >
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="size-4">
              <path fillRule="evenodd" d="M12 3.75a.75.75 0 0 1 .75.75v6.75h6.75a.75.75 0 0 1 0 1.5h-6.75v6.75a.75.75 0 0 1-1.5 0v-6.75H4.5a.75.75 0 0 1 0-1.5h6.75V4.5a.75.75 0 0 1 .75-.75Z" clipRule="evenodd" />
              </svg>
              Widget
            </button>
          </div>
          <div className="card card-border bg-base-100 w-141 justify-end mb-4">
            <div className="card-body">
              <h2 className="card-title">Links
                <button onClick={openModal}>
                  {/* change to add link */}
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
              </h2>
              <div className="justify-center">
                <button className="btn btn-link">
                  Roles
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 6h9.75M10.5 6a1.5 1.5 0 1 1-3 0m3 0a1.5 1.5 0 1 0-3 0M3.75 6H7.5m3 12h9.75m-9.75 0a1.5 1.5 0 0 1-3 0m3 0a1.5 1.5 0 0 0-3 0m-3.75 0H7.5m9-6h3.75m-3.75 0a1.5 1.5 0 0 1-3 0m3 0a1.5 1.5 0 0 0-3 0m-9.75 0h9.75" />
                  </svg>
                </button>
                <button className="btn btn-link">
                  File Explorer
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 12.75V12A2.25 2.25 0 0 1 4.5 9.75h15A2.25 2.25 0 0 1 21.75 12v.75m-8.69-6.44-2.12-2.12a1.5 1.5 0 0 0-1.061-.44H4.5A2.25 2.25 0 0 0 2.25 6v12a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 18V9a2.25 2.25 0 0 0-2.25-2.25h-5.379a1.5 1.5 0 0 1-1.06-.44Z" />
                  </svg>
                </button>
                <button className="btn btn-link">
                  Reports
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 17.25v3.375c0 .621-.504 1.125-1.125 1.125h-9.75a1.125 1.125 0 0 1-1.125-1.125V7.875c0-.621.504-1.125 1.125-1.125H6.75a9.06 9.06 0 0 1 1.5.124m7.5 10.376h3.375c.621 0 1.125-.504 1.125-1.125V11.25c0-4.46-3.243-8.161-7.5-8.876a9.06 9.06 0 0 0-1.5-.124H9.375c-.621 0-1.125.504-1.125 1.125v3.5m7.5 10.375H9.375a1.125 1.125 0 0 1-1.125-1.125v-9.25m12 6.625v-1.875a3.375 3.375 0 0 0-3.375-3.375h-1.5a1.125 1.125 0 0 1-1.125-1.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H9.75" />
                  </svg>
                </button>
                <button className="btn btn-link">
                  Trends
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 18 9 11.25l4.306 4.306a11.95 11.95 0 0 1 5.814-5.518l2.74-1.22m0 0-5.94-2.281m5.94 2.28-2.28 5.941" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
          <div className="card card-border bg-base-100 w-141 justify-end mb-4">
            <div className="card-body">
              <h2 className="card-title">Data Overview</h2>
            </div>
          </div>
          <div className="card card-border bg-base-100 w-141 justify-end mb-4">
            <div className="card-body">
              <h2 className="card-title">Graph</h2>
            </div>
          </div>
        </div>
      </div>

      <CreateProject isOpen={isModalOpen} onClose={closeModal} />
    </div>
  );
};

export default Projects;
