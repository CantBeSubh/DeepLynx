"use client";

import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import CreateProject from "@/app/(home)/components/CreateProjectsWidget";
import { sampleProjectData } from "@/app/(home)/dummy_data/data";
import { ProjectsList } from "@/app/(home)/types/types";
import { ExpandableTable } from "@/app/(home)/components/Accordion";
import { useProjectSession } from "@/app/contexts/ProjectSessionContext";
import { useUserSession } from "@/app/contexts/UserSessionContext";

const LandingPage = () => {
  const router = useRouter();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [tableData, setTableData] = useState<ProjectsList[]>(sampleProjectData);
  const { setProject } = useProjectSession();
  const { user } = useUserSession();

  const openModal = () => setIsModalOpen(true);
  const closeModal = () => setIsModalOpen(false);

  const handleExplore = (project: ProjectsList) => {
    setProject({ projectId: project.id ?? "", projectName: project.name });
    router.push(`/project/${project.id}`);
  };

  // useEffect(() => {
  //   if (!user?.isLoggedIn) {
  //     router.push("/login");
  //   }
  // }, [user]);

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
                      iconPath:
                        "M4.098 19.902a3.75 3.75 0 0 0 5.304 0l6.401-6.402M6.75 21A3.75 3.75 0 0 1 3 17.25V4.125C3 3.504 3.504 3 4.125 3h5.25c.621 0 1.125.504 1.125 1.125v4.072M6.75 21a3.75 3.75 0 0 0 3.75-3.75V8.197M6.75 21h13.125c.621 0 1.125-.504 1.125-1.125v-5.25c0-.621-.504-1.125-1.125-1.125h-4.072M10.5 8.197l2.88-2.88c.438-.439 1.15-.439 1.59 0l3.712 3.713c.44.44.44 1.152 0 1.59l-2.879 2.88M6.75 17.25h.008v.008H6.75v-.008Z",
                    },
                    {
                      title: "Data Records",
                      value: 30,
                      iconPath:
                        "M20.25 6.375c0 2.278-3.694 4.125-8.25 4.125S3.75 8.653 3.75 6.375m16.5 0c0-2.278-3.694-4.125-8.25-4.125S3.75 4.097 3.75 6.375m16.5 0v11.25c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125V6.375m16.5 0v3.75m-16.5-3.75v3.75m16.5 0v3.75C20.25 16.153 16.556 18 12 18s-8.25-1.847-8.25-4.125v-3.75m16.5 0c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125",
                    },
                    {
                      title: "Connections",
                      value: 1250,
                      iconPath:
                        "M7.5 21 3 16.5m0 0L7.5 12M3 16.5h13.5m0-13.5L21 7.5m0 0L16.5 12M21 7.5H7.5",
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
                  <button
                    className="btn btn-secondary text-primary-content mt-4"
                    onClick={() => router.push(`project/${project.id}`)}
                  >
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

export default LandingPage;
