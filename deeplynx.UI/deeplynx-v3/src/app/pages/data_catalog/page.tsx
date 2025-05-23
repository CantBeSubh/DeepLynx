"use client";

import LargeSearchBar from "@/app/components/LargeSearchBar";
import React, { useState, useEffect } from "react";
import { fileTableData } from "@/app/dummy_data/data";
import GenericTable from "@/app/components/GenericTable";
import { Column, TableRow, FileViewerTableRow } from "@/app/types/types";

const DataCatalog = () => {
  // State to manage table data, project name, mount status, search term, active filters, and next filter ID
  const [tableData, setTableData] =
    useState<FileViewerTableRow[]>(fileTableData);
  const [projectName, setProjectName] = useState<string>("");
  const [hasMounted, setHasMounted] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState<number>(1);

  // Effect to set project name from localStorage and mark the component as mounted
  useEffect(() => {
    setHasMounted(true);
    const storedName = localStorage.getItem("selectedProjectName");
    if (storedName) setProjectName(storedName);
  }, []);

  // Effect to filter table data based on active filters
  useEffect(() => {
    if (activeFilters.length === 0) {
      // If no filters are active, show all data
      setTableData(fileTableData);
    } else {
      // Apply filters to the data
      const filteredData = fileTableData.filter((file) => {
        return activeFilters.some((filter) =>
          file.fileName.toLowerCase().includes(filter.term.toLowerCase())
        );
      });
      setTableData(filteredData);
    }
  }, [activeFilters]);

  // Function to handle search input and add it as an active filter
  const handleSearch = (value: string) => {
    if (
      !value.trim() || // Ignore empty searches
      activeFilters.some((filter) => filter.term === value.trim()) // Ignore duplicates
    ) {
      return;
    }
    setActiveFilters([
      ...activeFilters,
      { id: nextFilterId, term: value.trim() }, // Add new filter
    ]);
    setNextFilterId(nextFilterId + 1); // Increment filter ID for next filter
    setSearchTerm(""); // Clear search input
  };

  // Function to remove a specific filter by its ID
  const removeFilter = (filterId: number) => {
    setActiveFilters(activeFilters.filter((filter) => filter.id !== filterId));
  };

  // Function to clear all active filters
  const clearAllFilters = () => {
    setActiveFilters([]); // Reset filters
    setSearchTerm(""); // Clear search input
  };

  // Define columns for the GenericTable
  const columns: Column[] = [
    {
      header: "File Name",
      accessor: "fileName", // Accessor for file name data
    },
    {
      header: "Explore",
      accessor: "explore", // Accessor for explore action
      cell: (row: TableRow) => (
        <button className="btn btn-primary btn-outline btn-xs">Explore</button> // Button to explore file
      ),
    },
  ];

  // If the component has not mounted yet, return null to avoid rendering
  if (!hasMounted) {
    return null;
  }

  return (
    <div>
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">
            {hasMounted ? `Project Name: ${projectName}` : "Loading..."}
          </h1>
        </div>
      </div>
      <div className="divider"></div>
      <div className="flex justify-center">
        {/* Search bar for filtering files */}
        <LargeSearchBar
          onEnter={handleSearch} // Call handleSearch on Enter key
          placeholder="Search files by name..."
          value={searchTerm} // Controlled input for search term
          onChange={(e) => setSearchTerm(e.target.value)} // Update search term on input change
        />
      </div>
      {/* Display active filters */}
      {activeFilters.length > 0 && (
        <div className="max-w-3xl mx-auto mb-6 mt-3">
          <div className="flex flex-wrap items-center gap-2">
            {activeFilters.map((filter) => (
              <div
                key={filter.id}
                className="bg-base-200 rounded-full px-3 py-1 flex items-center gap-2 text-sm"
              >
                <span>Filtered by: {filter.term}</span>
                <button
                  onClick={() => removeFilter(filter.id)} // Remove filter on button click
                  className="hover:text-error"
                >
                  x
                </button>
              </div>
            ))}
            {/* Button to clear all active filters if more than one is applied */}
            {activeFilters.length > 1 && (
              <button
                onClick={clearAllFilters}
                className="text-sm hover:underline ml-2"
              >
                Clear all
              </button>
            )}
          </div>
        </div>
      )}
      {/* Show message if no results found based on active filters */}
      {activeFilters.length > 0 && tableData.length === 0 && (
        <div>
          <p>No results found.</p>
        </div>
      )}
      {/* Show number of matches found based on active filters */}
      {activeFilters.length >= 1 && (
        <div className="border-b border-base-200">
          <h2>Found {tableData.length} matches</h2>
        </div>
      )}
      {/* Render the GenericTable with filtered data */}
      <GenericTable columns={columns} data={tableData} />
    </div>
  );
};

export default DataCatalog;
