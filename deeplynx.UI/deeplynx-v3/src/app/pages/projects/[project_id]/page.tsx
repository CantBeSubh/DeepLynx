"use client";

import React, { useState, useEffect } from "react";

const ProjectDetailPage = () => {
  // State to manage the project name and mounting status
  const [projectName, setProjectName] = useState<string>(""); // Initialize project name as an empty string
  const [hasMounted, setHasMounted] = useState(false); // State to check if the component has mounted

  // Effect to run on component mount
  useEffect(() => {
    setHasMounted(true); // Set the mounted state to true
    const storedName = localStorage.getItem("selectedProjectName"); // Retrieve the project name from localStorage
    if (storedName) setProjectName(storedName); // If a name is found, set it to the project name state
  }, []);

  // If the component has not mounted yet, return null to avoid rendering, i do this to try and control the Hydration of pages ... its a console error
  if (!hasMounted) {
    return null;
  }

  return (
    <div>
      <main>
        {/* Header section */}
        <div>
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-2xl font-bold">
                {hasMounted ? `Project Name: ${projectName}` : "Loading..."}{" "}
                {/* Display project name or loading text */}
              </h1>
            </div>
          </div>
          <div className="divider"></div> {/* Divider line */}
        </div>

        {/* Project Overview Card */}
        <div className="mb-6">
          <div className="card w-120 bg-base-100 card-sm shadow-sm">
            {" "}
            {/* Card component for project overview */}
            <div className="card-body">
              <h2 className="card-title">Project Description</h2>{" "}
              {/* Title for project description */}
              <p>
                A card component has a figure, a body part, and inside body
                there are title and actions parts {/* Example description */}
              </p>
              <div className="justify-end card-actions">
                <button className="btn btn-primary btn-dash btn-xs">
                  Edit
                </button>{" "}
                {/* Button to edit project details */}
              </div>
            </div>
          </div>
        </div>

        {/* Section for displaying personnel data */}
        <div>
          <h2 className="text-lg font-semibold mb-4">Personnel</h2>{" "}
          {/* Title for personnel section */}
          <div className="divider"></div> {/* Divider line */}
        </div>

        {/* TODO: Data Management? (Placeholder for future implementation) */}
      </main>
    </div>
  );
};

export default ProjectDetailPage;
