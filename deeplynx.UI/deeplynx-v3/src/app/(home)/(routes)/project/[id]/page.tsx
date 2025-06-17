"use client";

import React, { useState, useEffect } from "react";
import { sampleProjectData } from "@/app/(home)/dummy_data/data";
import { useParams } from "next/navigation";
import { ProjectsList } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionContext";

const ProjectDetailPage = () => {
  const { id } = useParams();
  const projectId = id?.toString();
  const [project, setProject] = useState<ProjectsList | null>(null);
  const { setProject: setProjectSession, hasLoaded } = useProjectSession();

  useEffect(() => {
    if (!hasLoaded || !projectId) return;

    const found = sampleProjectData.find((p) => p.id === projectId);
    if (found) {
      setProject(found);
      setProjectSession({ projectId: found.id, projectName: found.name });
    }
  }, [hasLoaded, projectId]);

  if (!hasLoaded) return <p className="p-4">Loading session...</p>;
  if (!project) return <p className="p-4">No project found.</p>;

  return (
    <div>
      <main>
        <div className="flex justify-between items-center text-secondary-content">
          <h1 className="text-2xl font-bold">Project Name: {project.name}</h1>
        </div>
        <div className="divider"></div>

        <div className="mb-6">
          <div className="card w-120 bg-base-200 text-secondary-content card-sm shadow-sm">
            <div className="card-body">
              <h2 className="card-title">Project Description</h2>
              <p>{project.description}</p>
              <div className="justify-end card-actions">
                <button className="btn btn-accent btn-outline btn-xs">
                  Edit
                </button>
              </div>
            </div>
          </div>
        </div>

        <div>
          <h2 className="text-lg font-semibold mb-4 text-secondary-content">
            Personnel
          </h2>
          <div className="divider"></div>
        </div>
      </main>
    </div>
  );
};

export default ProjectDetailPage;
