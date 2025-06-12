"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import React, { useState, useEffect } from "react";
<<<<<<<< HEAD:deeplynx.UI/deeplynx-v3/src/app/(routes)/data_catalog/page.tsx
import { fileTableData } from "@/app/dummy_data/data";
import GenericTable from "@/app/components/GenericTable";
import { Column, FileViewerTableRow } from "@/app/types/types";
import { useRouter } from "next/navigation";
========
import { fileTableData } from "@/app/(home)/dummy_data/data";
import GenericTable from "@/app/(home)/components/GenericTable";
import {
  Column,
  TableRow,
  FileViewerTableRow,
  ProjectsList,
} from "@/app/(home)/types/types";
>>>>>>>> origin/develop:deeplynx.UI/deeplynx-v3/src/app/(home)/pages/data_catalog/page.tsx

const DataCatalog = () => {
  const router = useRouter();
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

  useEffect(() => {
    if (activeFilters.length === 0) {
      setTableData(fileTableData);
    } else {
      const filtered = fileTableData.filter((row) => {
        return activeFilters.some((filter) => {
          const query = filter.term.toLowerCase();
          return (
            row.fileName.toLowerCase().includes(query) ||
            row.fileDescription.toLowerCase().includes(query) ||
            row.fileType.toLowerCase().includes(query) ||
            row.tags.some((tag) => tag.toLowerCase().includes(query)) ||
            (row.timeseries ? "yes" : "no").includes(query) ||
            row.lastEdit.toLowerCase().includes(query)
          );
        });
      });
      setTableData(filtered);
    }
  }, [activeFilters]);

  const getHighlightedCell = (text: unknown, queries: string[]) => {
    const safeText = String(text); // ⛑ force conversion to string

    if (!queries.length) return { content: safeText, matched: false };

    const lowerText = safeText.toLowerCase();
    const match = queries.find((q) => lowerText.includes(q.toLowerCase()));

    if (!match) return { content: safeText, matched: false };

    const regex = new RegExp(`(${match})`, "gi");
    const parts = safeText.split(regex);

    const content = parts.map((part, index) =>
      regex.test(part) ? (
        <span key={index} className="font-bold text-info-content rounded px-1">
          {part}
        </span>
      ) : (
        part
      )
    );

    return { content, matched: true };
  };

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

  const activeSearchTerms = activeFilters.map((f) => f.term.toLowerCase());

  // Define columns for the GenericTable
  const columns: Column<FileViewerTableRow>[] = [
    {
      data: "fileName",
      cell: (row: FileViewerTableRow) => {
        const fileName = getHighlightedCell(row.fileName, activeSearchTerms);
        const fileDescription = getHighlightedCell(
          row.fileDescription,
          activeSearchTerms
        );
        const cellBg =
          fileName.matched || fileDescription.matched ? "bg-info/40" : "";
        return (
          <div className={`p-2 rounded ${cellBg}`}>
            <div className="font-bold">{fileName.content}</div>
            <div className="text-sm text-base-300">
              {fileDescription.content}
            </div>
          </div>
        );
      },
    },
    {
      sortable: false,
      cell: (row: FileViewerTableRow) => {
        const cellData = getHighlightedCell(row.fileType, activeSearchTerms);
        return (
          <div>
            <div
              className={`p-2 rounded ${cellData.matched ? "bg-info/40" : ""}`}
            >
              <div>{cellData.content}</div>
            </div>
          </div>
        );
      },
    },
    {
      sortable: false,
      cell: (row: FileViewerTableRow) => {
        const cellData = getHighlightedCell(row.tags, activeSearchTerms);
        return (
          <div>
            <div
              className={`p-2 rounded ${cellData.matched ? "bg-info/40" : ""}`}
            >
              <div>{cellData.content}</div>
            </div>
          </div>
        );
      },
    },
    {
      sortable: false,
      cell: (row: FileViewerTableRow) => {
        const cellData = getHighlightedCell(row.timeseries, activeSearchTerms);
        return (
          <div>
            <div
              className={`p-2 rounded ${cellData.matched ? "bg-info/40" : ""}`}
            >
              <div>{cellData.content}</div>
            </div>
          </div>
        );
      },
    },
    {
      sortable: false,
      cell: (row: FileViewerTableRow) => {
        const cellData = getHighlightedCell(row.lastEdit, activeSearchTerms);
        return (
          <div>
            <div
              className={`p-2 rounded ${cellData.matched ? "bg-info/40" : ""}`}
            >
              <div>{cellData.content}</div>
            </div>
          </div>
        );
      },
    },
    {
      sortable: false,
      cell: (row: FileViewerTableRow) => (
        <div className="flex justify-end mr-10">
          <button
            className="btn btn-primary-content btn-outline"
            onClick={() => router.push(`/file_details/${row.id}`)}
          >
            Explore
          </button>
        </div>
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
          <h1 className="text-2xl font-bold text-secondary-content">
            {hasMounted ? `Project Name: ${projectName}` : "Loading..."}
          </h1>
        </div>
      </div>
      <div className="divider"></div>
      <div className="flex justify-center">
        {/* Search bar for filtering files */}
        <LargeSearchBar
          onEnter={handleSearch} // Call handleSearch on Enter key
          placeholder="Search table ..."
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
      <GenericTable
        columns={columns}
        data={tableData}
        rowClassName="border-separate border-spacing-y-2"
      />
    </div>
  );
};

export default DataCatalog;
