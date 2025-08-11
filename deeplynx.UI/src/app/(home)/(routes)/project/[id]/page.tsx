"use client";

import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import SavedSearchesTabs from "@/app/(home)/components/SavedSearches";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { ProjectsList } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { getProject } from "@/app/lib/projects_services";
import { Cog6ToothIcon, PlusIcon } from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import RecentRecordsCard from "../../data_catalog/RecentRecordsCard";
import SearchInput from "@/app/(home)/components/SearchInput";

const ProjectDetailPage = () => {
  const router = useRouter();
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
  }, [hasLoaded, projectId, setProjectSession]);

  if (!hasLoaded) return <p className="p-4">Loading session...</p>;
  if (!project) return <p className="p-4">No project found.</p>;

  return (
    <div>
      <main>
        <div className="text-info-content bg-base-200/40 pl-12 py-2">
          <h1 className="text-2xl">Project Name: {project.name}</h1>
          <p className="mt-2 text-base-content">{project.description}</p>
          <p>
            <strong>Created: </strong>
            {project?.createdAt &&
              format(new Date(project.createdAt), "MM/dd/yyyy")}
          </p>
        </div>

        <div className="flex w-full mt-6">
          <div className="w-full md:w-3/5 px-4">
            <SearchInput
              className="md:w-1/2 mb-4 rounded-2xl"
              placeholder="Search Recently Added Records"
            />
            <div className="card card-border">
              <div className="card-body">
                <div className="flex justify-between px-4">
                  <h1 className="text-xl font-semibold">
                    Data Catalog Overview
                  </h1>

                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => {
                      const query = new URLSearchParams({
                        fromProject: projectId || "",
                      }).toString();
                      router.push(`/data_catalog?${query}`);
                    }}
                  >
                    Visit
                  </button>
                </div>
                <RecentRecordsCard
                  selectedProjects={projectId ? [projectId] : []}
                />
              </div>
            </div>

            <SavedSearchesTabs />
          </div>

          <div className="w-full md:w-2/5 px-4">
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
