"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { useEffect, useState } from "react";
import { dropDownProjects, fileTableData } from "@/app/(home)/dummy_data/data";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";
import { QueueListIcon, TableCellsIcon } from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import GridView from "./GridView";
import ListView from "./ListView";
import ProjectDropdown from "./ProjectDropdown";
import RecentRecordsCard from "./RecentRecordsCard";

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
  const [viewMode, setViewMode] = useState<"list" | "table">("list");

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

  const gridViewColumns: Column<FileViewerTableRow>[] = [
    {
      header: "ID",
      data: "id",
    },
    {
      header: "Record Name",
      data: "fileName",
    },
    {
      header: "Class",
      cell: (row) =>
        row.timeseries ? (
          <span className="badge text-sm">TimeSeries</span>
        ) : (
          <span></span>
        ),
    },
    {
      header: "Tags",
      cell: (row) => {
        const [expanded, setExpanded] = useState(false);
        const tagsToShow = expanded ? row.tags : row.tags.slice(0, 3);

        return (
          <div className="flex flex-wrap gap-1">
            {tagsToShow.map((tag, index) => (
              <span className="badge text-sm" key={index}>
                {tag}
              </span>
            ))}
            {row.tags.length > 3 && !expanded && (
              <button
                className="text-sm badge badge-secondary badge-outline text-secondary ml-2"
                onClick={() => setExpanded(true)}
              >
                See more
              </button>
            )}
          </div>
        );
      },
    },
    {
      header: "Associated Records",
      cell: (row) => {
        const [expanded, setExpanded] = useState(false);
        const recordsToShow = expanded
          ? row.associatedRecords
          : row.associatedRecords?.slice(0, 2) || [];

        return (
          <div className="flex flex-col gap-2">
            {recordsToShow?.map((rec, index) => (
              <a key={index} className="text-blue-600 underline text-sm">
                {rec}
              </a>
            ))}
            {row.associatedRecords &&
              row.associatedRecords.length > 3 &&
              !expanded && (
                <button
                  className="text-sm flex badge text-blue-600 ml-2"
                  onClick={() => setExpanded(true)}
                >
                  See more
                </button>
              )}
          </div>
        );
      },
    },
    {
      header: "Last Edited",
      data: "dateModified",
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
            Data Catalog
          </h1>
          <ProjectDropdown projects={dropDownProjects} />
        </div>
      </div>
      <div className="divider"></div>
      <div className="flex justify-between items-center gap-4 mb-4">
        <LargeSearchBar
          placeholder="Seach by name and description ... happy?"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onEnter={handleSearch}
          activeFilters={activeFilters}
          onRemoveFilter={removeFilter}
          onClearAll={clearAllFilters}
          resultCount={tableData.length}
          showResultsMessage={activeFilters.length > 0}
          className="md:w-1/4"
        />
        <div className="flex">
          <button
            className={`btn btn-sm ${
              viewMode === "list" ? "btn-primary" : "btn-ghost"
            }`}
            onClick={() => setViewMode("list")}
          >
            <QueueListIcon className="size-7" />
          </button>
          <button
            className={`btn btn-sm ${
              viewMode === "table" ? "btn-primary" : "btn-ghost"
            }`}
            onClick={() => setViewMode("table")}
          >
            <TableCellsIcon className="size-7" />
          </button>
        </div>
      </div>
      {viewMode === "list" ? (
        <ListView records={tableData} activeSearchTerms={activeSearchTerms} />
      ) : (
        <GridView
          columns={gridViewColumns}
          data={tableData}
          activeSearchTerms={activeSearchTerms}
        />
      )}
      {/* {activeSearchTerms < 1 && (<RecentRecordsCard records={[]} />)} */}
    </div>
  );
};

export default DataCatalog;
