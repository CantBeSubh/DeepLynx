"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import SavedSearchesTabs from "@/app/(home)/components/SavedSearches";
import { ProjectsList } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { getProject } from "@/app/lib/projects_services";
import { format } from "date-fns";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";

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
                <SavedSearchesTabs />
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
