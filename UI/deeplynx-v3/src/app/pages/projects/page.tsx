"use client";
import React, { useState } from "react";
import CreateProject from "./CreateProjectsWidget";
import { useRouter } from "next/navigation";
import SearchInput from "@/app/components/SearchInput";
import { sampleProjectData } from "@/app/dummy_data/data";
import SideMenu from "@/app/components/SideMenu";

const Projects = () => {
  const router = useRouter(); // Initialize the router

  // State for controlling the modal visibility
  const [isModalOpen, setIsModalOpen] = useState(false);
  const openModal = () => setIsModalOpen(true);
  const closeModal = () => setIsModalOpen(false);

  // State for controlling the sidebar collapse state
  const [isMenuCollapsed, setIsMenuCollapsed] = useState<boolean>(false);
  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  // State for the search query
  const [searchQuery, setSearchQuery] = useState("");

  // Filter projects based on the search query
  const filteredProjects = sampleProjectData.filter((project) =>
    project.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="flex min-h-screen">
      <SideMenu onToggle={handleMenuToggle} />{" "}
      {/* Render the SideMenu component */}
      <div
        className={`flex-grow transition-all duration-300 ease-in-out ${
          isMenuCollapsed ? "ml-22" : "ml-64"
        } flex-1 p-6 max-w-auto mx-auto`}
      >
        <main className="p-6">
          {/* Title and search bar */}
          <div>
            <div className="flex justify-between items-center">
              <div className=" text-base-content">
                <h1 className="text-2xl font-bold">Welcome Back Kevin</h1>
              </div>
              {/* 
                  TODO: Expand the searchability. Right now it can filter titles only.
                  Feed back: Being able to search the description of cards. 
              */}
              <SearchInput
                placeholder="Search projects ..."
                onChange={(e) => setSearchQuery(e.target.value)}
                className="placeholder:text-base-content"
              />
            </div>
            <div className="divider"></div>
          </div>

          {/* Your Projects section */}
          <div>
            <div className="flex justify-between items-center">
              <h3>Your Projects</h3>
              <button
                className="btn btn-outline btn-sm btn-primary"
                onClick={openModal} // Open the modal when clicked
              >
                Add new Project
              </button>
            </div>
            <div className="divider"></div>
          </div>

          {/* Project Grid */}
          <div className="grid sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">
            {filteredProjects.map((project, index) => (
              <div key={index} className="card bg-base-200 shadow-md p-4">
                <h3 className="font-bold text-base-content">{project.name}</h3>
                <p className="text-sm text-base-content">
                  {project.description}
                </p>
                <div className="mt-4">
                  <button
                    className="btn bg-primary btn-block text-base-100"
                    onClick={() => router.push(`/pages/projects/${project.id}`)} // Navigate to the project page
                  >
                    enter project
                  </button>
                </div>
              </div>
            ))}
          </div>

          {/* Recent Activity section */}
          <h3 className="">Your Recent Activity</h3>
          <div className="divider"></div>
        </main>
      </div>
      {/* Render the CreateProject modal */}
      <CreateProject isOpen={isModalOpen} onClose={closeModal} />
    </div>
  );
};

export default Projects;
