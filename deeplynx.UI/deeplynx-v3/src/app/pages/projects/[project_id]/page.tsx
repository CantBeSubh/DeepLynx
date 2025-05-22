"use client";

import React, { useState, useEffect } from "react";

const ProjectDetailPage = () => {
  const [projectName, setProjectName] = useState<string>("");
  const [hasMounted, setHasMounted] = useState(false);

  useEffect(() => {
    setHasMounted(true);
    const storedName = localStorage.getItem("selectedProjectName");
    if (storedName) setProjectName(storedName);
  }, []);

  if (!hasMounted) {
    return null;
  }

  return (
    <div>
      <main>
        {/* Header */}
        <div>
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-2xl font-bold">
                {hasMounted ? `Project Name: ${projectName}` : "Loading..."}
              </h1>
            </div>
          </div>
          <div className="divider"></div>
        </div>

        {/* Project Overview Card */}
        <div className="mb-6">
          <div className="card w-120 bg-base-100 card-sm shadow-sm">
            <div className="card-body">
              <h2 className="card-title">Project Description</h2>
              <p>
                A card component has a figure, a body part, and inside body
                there are title and actions parts
              </p>
              <div className="justify-end card-actions">
                <button className="btn btn-primary btn-dash btn-xs">
                  Edit
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* Your Data */}
        <div>
          <h2 className="text-lg font-semibold mb-4">Personnel</h2>
          <div className="divider"></div>
        </div>

        {/* TODO: Data Management? */}
      </main>
    </div>
  );
};

export default ProjectDetailPage;
