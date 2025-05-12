"use client";
import React, { useState } from "react";
import Sidebar from "./Sidebar";
import CreateProject from "./CreateProjectsWidget";
import { useRouter } from "next/navigation";
import SearchInput from "@/app/components/SearchInput";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import { sampleProjectData } from "@/app/dummy_data/data";

export default function Projects() {
  const router = useRouter();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const openModal = () => setIsModalOpen(true);
  const closeModal = () => setIsModalOpen(false);
  // ---------- Sidebar toggel effects ----------
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  const toggleSidebar = () => {
    setIsSidebarOpen(!isSidebarOpen);
  };
  // ---------- Sidebar toggel effects ends ----------

  const [searchQuery, setSearchQuery] = useState("");
  const filteredProjects = sampleProjectData.filter((project) =>
    project.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // TODO: When page gets smaller, change the grid layout of the cards to keep formatting
  return (
    <div className="flex min-h-screen bg-base-100">
      <Sidebar isOpen={isSidebarOpen} toggleSidebar={toggleSidebar} />
      <div
        className={`flex-grow transition-all duration-300 ease-in-out ${
          isSidebarOpen ? "ml-[32%]" : "ml-[2%]"
        }`}
      >
        <main className="container mx-auto p-6">
          {/* Title and search bar */}
          <div className="flex justify-between items-center mb-6">
            <div className="border-b-2 text-base-content">
              <h1 className="text-2xl font-bold">Your Projects</h1>
            </div>
            <SearchInput
              placeholder="Search"
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
          {/* Project Grid */}
          <div className="grid grid-cols-3 gap-6">
            {/* Create New Project Card */}
            <div className="card bg-neutral-content shadow-md flex items-center justify-center p-6">
              <button onClick={openModal}>
                <AddCircleIcon className="text-primary" sx={{ fontSize: 60 }} />
              </button>
              <p className="mt-4 font-bold text-neutral">Create New Project</p>
            </div>

            {filteredProjects.map((project, index) => (
              <div
                key={index}
                className="card bg-neutral-content shadow-md p-4"
              >
                <h3 className="font-bold text-neutral">{project.name}</h3>
                <p className="text-sm text-neutral">{project.description}</p>
                <div className="mt-4 flex justify-between">
                  <button className="btn btn-outline btn-sm text-neutral">
                    more info
                  </button>
                  <button
                    className="btn bg-primary btn-sm text-neutral-content"
                    onClick={() => router.push(`/pages/projects/${project.id}`)}
                  >
                    enter project
                  </button>
                </div>
              </div>
            ))}
          </div>
        </main>
        <button className="fixed bottom-10 right-10" onClick={openModal}>
          <AddCircleIcon className="text-primary" sx={{ fontSize: 60 }} />
        </button>
      </div>
      <CreateProject isOpen={isModalOpen} onClose={closeModal} />
    </div>
  );
}
