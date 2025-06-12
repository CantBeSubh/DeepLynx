"use client";

import React, { useState, useEffect } from "react";
import { sampleProjectData } from "@/app/dummy_data/data";
import { useParams } from "next/navigation";
import { ProjectsList } from "@/app/types/types";

const ProjectDetailPage = () => {
  // State to manage the project name and mounting status
  const [projectName, setProjectName] = useState<string>(""); // Initialize project name as an empty string
  const [hasMounted, setHasMounted] = useState(false); // State to check if the component has mounted
  const params = useParams();
  const projectId = params?.id as string;

  const [project, setProject] = useState<ProjectsList | null>(null);

  // Effect to run on component mount
  useEffect(() => {
    if (projectId) {
      const found = sampleProjectData.find((p) => p.id === projectId);
      setProject(found || null);
    }
  }, [projectId]);

  // If the component has not mounted yet, return null to avoid rendering, i do this to try and control the Hydration of pages ... its a console error
  if (!project) return <p className="p-4">Loading project ...</p>;

  return (
    <div>
      <main>
        {/* Header section */}
        <div className="flex justify-between items-center text-secondary-content">
          <h1 className="text-2xl font-bold">Project Name: {project.name}</h1>
        </div>
        <div className="divider"></div> {/* Divider line */}
        {/* Project Overview Card */}
        <div className="mb-6">
          <div className="card w-120 bg-base-200 text-secondary-content card-sm shadow-sm">
            {" "}
            {/* Card component for project overview */}
            <div className="card-body">
              <h2 className="card-title ">Project Description</h2>
              {/* Title for project description */}
              <p>{project.description}</p>
              <div className="justify-end card-actions">
                <button className="btn btn-accent btn-outline btn-xs">
                  Edit
                </button>{" "}
                {/* Button to edit project details */}
              </div>
            </div>
          </div>
        </div>
        {/* Section for displaying personnel data */}
        <div>
          <h2 className="text-lg font-semibold mb-4 text-secondary-content">
            Personnel
          </h2>{" "}
          {/* Title for personnel section */}
          <div className="divider"></div> {/* Divider line */}
        </div>
        {/* TODO: Data Management? (Placeholder for future implementation) */}
      </main>
    </div>
  );
};

export default ProjectDetailPage;
