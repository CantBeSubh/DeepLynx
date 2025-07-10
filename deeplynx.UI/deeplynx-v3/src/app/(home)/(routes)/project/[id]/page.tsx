"use client";

import React, { useState, useEffect } from "react";
import { peopleData } from "@/app/(home)/dummy_data/data";
import { useParams } from "next/navigation";
import { Column, ProjectsList } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import Link from "next/link";
import Tabs from "@/app/(home)/components/Tabs";
import GenericTable from "@/app/(home)/components/GenericTable";
import AvatarCell from "@/app/(home)/components/Avatar";
import { format } from "date-fns";
import { getProject } from "@/app/lib/api";

type PopularTable = {
  id: number;
  name: string;
  image: string;
  nickname: string;
  visibility: string;
};

const ProjectDetailPage = () => {
  const { id } = useParams();
  const projectId = id?.toString();
  const [project, setProject] = useState<ProjectsList | null>(null);
  const { setProject: setProjectSession, hasLoaded } = useProjectSession();

  useEffect(() => {
    if (!hasLoaded || !projectId) return;

    const fetchProject = async () => {
      try {
        const data = await getProject(projectId);
        setProject(data);
        setProjectSession({ projectId: data.id, projectName: data.name });
      } catch (error) {
        console.error("Failed to fetch project:", error);
      }
    };
    fetchProject();
  }, [hasLoaded, projectId, project, setProjectSession]);

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
      content: <></>,
    },
  ];

  if (!hasLoaded) return <p className="p-4">Loading session...</p>;
  if (!project) return <p className="p-4">No project found.</p>;

  return (
    <div>
      <main>
        <div className="text-secondary-content">
          <h1 className="text-2xl">Project Name: {project.name}</h1>
          <p className="mt-2 text-base-content">{project.description}</p>
          <p>
            <strong>Created: </strong>
            {project?.createdAt &&
              format(new Date(project.createdAt), "MM/dd/yyyy")}
          </p>
        </div>

        <div className="divider"></div>

        <div className="flex w-full">
          <div className="w-full md:w-1/2 pr-4">
            <div className="flex flex-col w-full max-w-2xl mx-auto">
              <LargeSearchBar />
              <div className="flex justify-end">
                <Link
                  href="#"
                  className="text-sm underline text-secondary/70 mr-3 hover:text-primary mt-1"
                >
                  Advanced Search
                </Link>
              </div>
            </div>

            <div className="card shadow-lg mt-3">
              <div className="card-body">
                <h2 className="card-title">Seaved Searchs</h2>
                <Tabs tabs={tabData} className="tabs tabs-border" />
              </div>
            </div>
          </div>
          <div>Other half here: 👇</div>
        </div>
      </main>
    </div>
  );
};

export default ProjectDetailPage;
