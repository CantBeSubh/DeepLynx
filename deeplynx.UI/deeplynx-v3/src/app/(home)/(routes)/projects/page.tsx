"use client";

import React, { useState } from "react";
import CreateProject from "./CreateProjectsWidget";
import { useRouter } from "next/navigation";
import { sampleProjectData } from "@/app/(home)/dummy_data/data";
import GenericTable from "@/app/(home)/components/GenericTable";
import { Column, ProjectsList } from "@/app/(home)/types/types";

const Projects = () => {
  const router = useRouter();

  // State for controlling the modal visibility
  const [isModalOpen, setIsModalOpen] = useState(false); // State to manage modal open/close
  const openModal = () => setIsModalOpen(true); // Function to open the modal
  const closeModal = () => setIsModalOpen(false); // Function to close the modal
  // cosnt addWidget = () => canAddWidget(true); //Function to add widgets

  // State to manage the table data
  const [tableData, setTableData] = useState<ProjectsList[]>(sampleProjectData);

  // Define the columns for the GenericTable component
  const columns: Column<ProjectsList>[] = [
    {
      header: "Project Name",
      data: "name",
    },
    {
      header: "Description",
      data: "description",
    },
    {
      header: "Last Viewed",
      data: "lastViewed",
    },
    {
      header: "", // Empty header for action buttons
      sortable: false, // Disable sorting for this column
      cell: (row) => (
        <button
          className="btn btn-sm btn-outline btn-accent"
          onClick={() => router.push(`/project/${row.id}`)} // Navigate to project details page
        >
          Explore
        </button>
      ),
    },
  ];

  // Render the Projects component
  return (
    <div className="bg-base-100">
      {/* Title and search bar  */}
      <div>
        <div className="flex justify-between items-center">
          <div className="text-secondary-content">
            <h1 className="text-2xl font-bold">Welcome Back Kevin</h1>{" "}
            {/* Welcome message */}
          </div>
        </div>
        <div className="divider"></div> {/* Divider line */}
      </div>
      {/* Your Projects section */}
      <div>
        <div className="flex justify-between items-center">
          <h3 className="text-secondary-content">Your Projects</h3>{" "}
          {/* Section title */}
          <button
            className="btn btn-outline btn-sm btn-accent"
            onClick={openModal} // Open the modal when clicked
          >
            Add new Project
          </button>
        </div>
        <div className="divider"></div> {/* Divider line */}
      </div>
      <div className="flex">
        <div className="">
          <div>
            <GenericTable
              columns={columns} // Pass columns to the table
              data={tableData} // Pass data to the table
              searchBar // Enable search bar
              filterPlaceholder="Search Projects ..." // Placeholder text for the search bar
            />
          </div>
        </div>
          <div className="overflow-x p-2">
            <div className="my-4 flex justify-between items-center justify-end">
              <button
                className="btn btn-outline btn-sm btn-accent"
                // onClick={addWidget} // Adds new widget when clicked
              >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" className="size-4">
                <path d="M8.75 3.75a.75.75 0 0 0-1.5 0v3.5h-3.5a.75.75 0 0 0 0 1.5h3.5v3.5a.75.75 0 0 0 1.5 0v-3.5h3.5a.75.75 0 0 0 0-1.5h-3.5v-3.5Z" />
                </svg>
                Widget
              </button>
            </div>
            <div className="card card-border bg-base-100 w-96">
              <div className="card-body">
                <h2 className="card-title">Links</h2>
                <div className="card-actions justify-end">
                  add links here
                </div>
              </div>
            </div>
            <div className="card card-border bg-base-100 w-96">
              <div className="card-body">
                <h2 className="card-title">Data Overview</h2>
                <div className="card-actions justify-end">
                  add data links here
                </div>
              </div>
            </div>
            <div className="card card-border bg-base-100 w-96">
              <div className="card-body">
                <h2 className="card-title">Graph</h2>
                <div className="card-actions justify-end">
                  add updated graphs here
                </div>
              </div>
            </div>
          </div>
      </div>
      {/* Render the GenericTable component */}

      {/* Render the CreateProject modal */}
      <CreateProject isOpen={isModalOpen} onClose={closeModal} />
    </div>
  );
};

export default Projects;
