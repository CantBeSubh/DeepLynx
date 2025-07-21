"use client";

import React, { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { ProjectsList } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import Link from "next/link";
import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { Cog6ToothIcon, PlusIcon } from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { getProject } from "@/app/lib/projects_services";
import SavedSearchesTabs from "@/app/(home)/components/SavedSearches";

const ProjectDetailPage = () => {
  const { id } = useParams();
  const projectId = id?.toString();
  const [project, setProject] = useState<ProjectsList | null>(null);
  const { setProject: setProjectSession, hasLoaded } = useProjectSession();
  const [widgetModal, setWidgetModal] = useState(false);
  const projectWidgets: WidgetType[] = [
    "RecentActivity",
    "ProjectOverview",
    "TeamMembers",
  ];

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
                <h2 className="card-title">Saved Searches</h2>
                <SavedSearchesTabs />
              </div>
            </div>
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
            <WidgetCard widgets={projectWidgets} />
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
