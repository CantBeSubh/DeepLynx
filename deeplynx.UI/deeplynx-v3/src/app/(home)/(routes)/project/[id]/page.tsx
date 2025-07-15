"use client";

import React, { useState, useEffect } from "react";
import { sampleProjectData, peopleData } from "@/app/(home)/dummy_data/data";
import { useParams, useRouter } from "next/navigation";
import { Column, ProjectsList } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import Link from "next/link";
import Tabs from "@/app/(home)/components/Tabs";
import GenericTable from "@/app/(home)/components/GenericTable";
import AvatarCell from "@/app/(home)/components/Avatar";
import CreateWidget from "@/app/(home)/components/CreateWidgets";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import {PlusIcon} from "@heroicons/react/24/outline"


type PopularTable = {
  id: number;
  name: string;
  image: string;
  nickname: string;
  visibility: string;
};

const ProjectDetailPage = () => {
  const router = useRouter();
  const { id } = useParams();
  const projectId = id?.toString();
  const [project, setProject] = useState<ProjectsList | null>(null);
  const { setProject: setProjectSession, hasLoaded } = useProjectSession();
  const [widgetModal, setWidgetModal] = useState(false);
  const projectWidgets: WidgetType[] = ["RecentActivity", "ProjectOverview", "TeamMembers"];

  useEffect(() => {
    if (!hasLoaded || !projectId) return;

    const found = sampleProjectData.find((p) => p.id === projectId);
    if (found) {
      setProject(found);
      setProjectSession({ projectId: found.id, projectName: found.name });
    }
  }, [hasLoaded, projectId]);

  const popular_table_columns: Column<PopularTable>[] = [
    {
      header: "Created by",
      cell: (row) => <AvatarCell name={row.name} image={row.image} />,
    },
    {
      header: "Search nickname",
      data: "nickname",
    },
    {
      header: "Visibility",
      data: "visibility",
    },
  ];

  const tabData = [
    {
      label: "Recent",
      content: <GenericTable columns={[]} data={[]} />,
    },
    {
      label: "Popular",
      content: (
        <GenericTable
          columns={popular_table_columns}
          data={peopleData}
          enablePagination
          rowsPerPage={5}
        />
      ),
    },
    {
      label: "My Searchs",
      content: "",
    },
  ];

  if (!hasLoaded) return <p className="p-4">Loading session...</p>;
  if (!project) return <p className="p-4">No project found.</p>;

  return (
    <div>
      <main>
        <div className="text-secondary-content">
          <h1 className="text-2xl">Project Name: {project.name}</h1>
          <p className="mt-2 text-base-content">
            For databases or systems tracking chain reactions and realated data.
          </p>
          <p>
            <strong>Created: </strong>
            {new Date(project?.created).toLocaleDateString("en-US", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
            })}
          </p>
        </div>

        <div className="divider"></div>

        <div className="flex w-full">
          <div className="w-full md:w-1/2 pr-4">
            <LargeSearchBar />
            <div className="flex justify-end">
              <Link
                href="#"
                className="text-sm underline text-secondary/70 mr-3 hover:text-primary mt-1"
              >
                Advanced Search
              </Link>
            </div>
            <div className="card shadow-lg mt-3">
              <div className="card-body">
                <h2 className="card-title">Seaved Searchs</h2>
                <Tabs tabs={tabData} className="tabs tabs-border" />
              </div>
            </div>
          </div>

          <div className="w-full md:w-1/2 px-4">
            <div className="flex justify-between items-center justify-end mb-4">
              <button className="btn btn-outline btn-secondary flex items-center mr-2">
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
                    d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z"
                  />
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z"
                  />
                </svg>
                Customize
              </button>
              <button
                onClick={() => setWidgetModal(true)}
                className="btn btn-secondary text-primary-content flex items-center"
              >
                <PlusIcon
                  className="size-6"/>
                Widget
              </button>
            </div>
          <WidgetCard widgets={projectWidgets}/>
        </div>
        </div>

        {/* Create Widget Modal */}
      <CreateWidget
        isOpen={widgetModal}
        onClose={() => setWidgetModal(false)}
      />

      </main>
    </div>
  );
};

export default ProjectDetailPage;
