"use client";

import React, { useState } from "react";
import CreateProject from "./CreateProjectsWidget";
import { useRouter } from "next/navigation";
import SearchInput from "@/app/(home)/components/SearchInput";
import { sampleProjectData } from "@/app/(home)/dummy_data/data";

const Projects = () => {
  const router = useRouter();

  // State for controlling the modal visibility
  const [isModalOpen, setIsModalOpen] = useState(false); // State to manage modal open/close
  const openModal = () => setIsModalOpen(true); // Function to open the modal
  const closeModal = () => setIsModalOpen(false); // Function to close the modal

  // State for the search query
  const [searchQuery, setSearchQuery] = useState(""); // State to hold the search query

  // Filter projects based on the search query
  const filteredProjects = sampleProjectData.filter(
    (project) => project.name.toLowerCase().includes(searchQuery.toLowerCase()) // Filter projects by name
  );

  return (
    <div className="flex">
      <main>
        {/* Title and search bar */}
        <div>
          <div className="flex justify-between items-center">
            <div className="text-base-content">
              <h1 className="text-2xl font-bold">Welcome Back Kevin</h1>{" "}
              {/* Welcome message */}
            </div>
            {/* 
                  TODO: Expand the searchability. Right now it can filter titles only.
                  Feedback: Being able to search the description of cards. 
              */}
            <SearchInput
              placeholder="Search projects ..."
              onChange={(e) => setSearchQuery(e.target.value)} // Update search query on input change
              className="placeholder:text-base-content"
            />
          </div>
          <div className="divider"></div> {/* Divider line */}
        </div>
        {/* Your Projects section */}
        <div>
          <div className="flex justify-between items-center">
            <h3>Your Projects</h3> {/* Section title */}
            <button
              className="btn btn-outline btn-sm btn-primary"
              onClick={openModal} // Open the modal when clicked
            >
              Add new Project
            </button>
          </div>
          <div className="divider"></div> {/* Divider line */}
        </div>
        {/* Project Grid */}
        <div className="grid sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">
          {filteredProjects.map(
            (
              project,
              index // Map through filtered projects to display them
            ) => (
              <div key={index} className="card card-outline shadow-md p-4">
                {" "}
                {/* Card for each project */}
                <h3 className="font-bold text-base-content">
                  {project.name}
                </h3>{" "}
                {/* Project name */}
                <p className="text-sm text-base-content">
                  {project.description}
                </p>{" "}
                {/* Project description */}
                <div className="mt-4">
                  <button
                    className="btn bg-secondary btn-block text-base-100"
                    onClick={() => {
                      localStorage.setItem("selectedProjectName", project.name); // Store selected project name in localStorage
                      router.push(`/pages/projects/${project.id}`); // Navigate to the project page
                    }}
                  >
                    enter project
                  </button>
                </div>
              </div>
            )
          )}
        </div>
        {/* Recent Activity section */}
        <h3 className="">Your Recent Activity</h3>{" "}
        {/* Section title for recent activity */}
        <div className="divider"></div> {/* Divider line */}
      </main>
      {/* Render the CreateProject modal */}
      <CreateProject isOpen={isModalOpen} onClose={closeModal} />{" "}
      {/* Pass modal open state and close function */}
    </div>
  );
};

export default Projects;
