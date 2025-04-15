"use client";
import React, { useState } from "react";
import Sidebar from "./Sidebar";
import CreateProject from "./CreateProjectsWidget";

export default function Projects() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const openModal = () => setIsModalOpen(true);
  const closeModal = () => setIsModalOpen(false);
  // ---------- Sidebar toggel effects ----------
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  const toggleSidebar = () => {
    setIsSidebarOpen(!isSidebarOpen);
  };
  // ---------- Sidebar toggel effects ends ----------

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
            <div className="relative">
              <input
                type="text"
                placeholder="Search"
                className="input input-border w-64 pl-10"
              />
              <svg
                className="absolute left-3 top-2.5 w-5 h-5 text-secondary"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  d="M21 21l-4.35-4.35m1.35-5.15a7 7 0 1 1-14 0 7 7 0 0 1 14 0z"
                />
              </svg>
            </div>
          </div>
          {/* Project Grid */}
          <div className="grid grid-cols-3 gap-6">
            {/* Create New Project Card */}
            <div className="card bg-neutral-content shadow-md flex items-center justify-center p-6">
              <button
                className="btn btn-circle btn-lg bg-primary text-neutral-content"
                onClick={openModal}
              >
                +
              </button>
              <p className="mt-4 font-bold text-neutral">
                Create New Project
              </p>
            </div>

            {/* Example Project Cards */}
            {Array.from({ length: 8 }).map((_, index) => (
              <div
                key={index}
                className="card bg-neutral-content shadow-md p-4"
              >
                <h3 className="font-bold text-neutral">admin</h3>
                <p className="text-sm text-neutral">
                  Here is a short description of this project and the things
                  that you might find inside of it. Max 150 char.
                </p>
                <div className="mt-4 flex justify-between">
                  <button className="btn btn-outline btn-sm text-neutral">
                    more info
                  </button>
                  <button className="btn bg-primary btn-sm text-neutral-content">
                    enter project
                  </button>
                </div>
              </div>
            ))}
          </div>
        </main>
        <button
          className="btn btn-circle bg-primary text-white fixed bottom-10 right-10 shadow-lg"
          onClick={openModal}
        >
          +
        </button>
      </div>
      <CreateProject isOpen={isModalOpen} onClose={closeModal} />
    </div>
  );
}
