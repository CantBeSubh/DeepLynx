"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { recentRecordsData } from "@/app/(home)/dummy_data/data";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { getAllRecords } from "@/app/lib/record_services";
import { getAllProjects } from "@/app/lib/projects_services";
import {
  ArrowUturnLeftIcon,
  EyeIcon,
  PlusIcon,
  QueueListIcon,
  TableCellsIcon,
} from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import ExpandableTagsCell from "./ExpandableTagCell";
import GridView from "./GridView";
import ListView from "./ListView";
import ProjectDropdown from "./ProjectDropdown";
import RecentRecordsCard from "./RecentRecordsCard";

const DataCatalog = () => {
  const router = useRouter();
  // State to manage table data, project name, mount status, search term, active filters, and next filter ID
  const [tableData, setTableData] = useState<FileViewerTableRow[]>([]);
  const [projectName, setProjectName] = useState<string>("");
  const [hasMounted, setHasMounted] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState<number>(1);
  const [viewMode, setViewMode] = useState<"list" | "table">("list");
  const [showAll, setShowAll] = useState(false);
  const { project, hasLoaded } = useProjectSession();
  const [projects, setProjects] = useState([]);

  // Effect to set project name from localStorage and mark the component as mounted
  useEffect(() => {
    setHasMounted(true);
    const storedName = localStorage.getItem("selectedProjectName");
    if (storedName) setProjectName(storedName);
  }, []);

  useEffect(() => {
    const fetchProjects = async () => {
      if (!hasLoaded || !project?.projectId) return;

      try {
        const data = await getAllProjects();
        const projectString = data.map((d: { name: number }) => d.name);
        setProjects(projectString);
      } catch (error) {
        console.error("Failed to fetch records:", error);
      }
    };
    fetchProjects();
  }, [project?.projectId, hasLoaded]);

  useEffect(() => {
    const fetchRecords = async () => {
      if (!hasLoaded || !project?.projectId) return;

      try {
        const data = await getAllRecords(project.projectId);
        const transformedRecords = data.map((d: { tags: string }) => {
          return {
            ...d,
            tags: JSON.parse(d.tags),
          };
        });
        setTableData(transformedRecords);
      } catch (error) {
        console.error("Failled to fetch records:", error);
      }
    };
    fetchRecords();
  }, [hasLoaded, project?.projectId]);

  // useEffect(() => {
  //   if (activeFilters.length === 0) {
  //     setTableData(fileTableData);
  //   } else {
  //     const filtered = fileTableData.filter((row) => {
  //       return activeFilters.some((filter) => {
  //         const query = filter.term.toLowerCase();
  //         return (
  //           row.fileName.toLowerCase().includes(query) ||
  //           row.fileDescription.toLowerCase().includes(query) ||
  //           row.fileType.toLowerCase().includes(query) ||
  //           row.tags.some((tag) => tag.toLowerCase().includes(query)) ||
  //           (row.timeseries ? "yes" : "no").includes(query) ||
  //           row.lastEdit.toLowerCase().includes(query)
  //         );
  //       });
  //     });
  //     setTableData(filtered);
  //   }
  // }, [activeFilters]);

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
      data: "name",
    },
    {
      header: "Class",
      cell: (row) =>
        row.className ? (
          <span className="badge text-sm">{row.className}</span>
        ) : (
          <span></span>
        ),
    },
    {
      header: "Tags",
      cell: (row) => <ExpandableTagsCell tags={row.tags} />,
    },
    // {
    //   header: "Associated Records",
    //   cell: (row) => <AssociatedRecordsCell records={row.associatedRecords} />,
    // },
    {
      header: "Last Edited",
      data: "modifiedAt",
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
          <ProjectDropdown projects={projects} />
        </div>
      </div>
      <div className="divider"></div>
      <div className="flex justify-between gap-4 mb-4">
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

        <div className="flex gap-4">
          {showAll ? (
            <button
              className="btn btn-outline btn-primary"
              onClick={() => {
                setShowAll(false);
                setViewMode("list");
                clearAllFilters();
              }}
            >
              <ArrowUturnLeftIcon className="size-6" />
              Show Recent Records
            </button>
          ) : (
            <button
              className="btn btn-outline btn-primary"
              onClick={() => {
                clearAllFilters();
                setViewMode("list");
                setShowAll(true);
              }}
            >
              <EyeIcon className="size-6" />
              Explore All Records
            </button>
          )}
          <button className="btn btn-primary text-white">
            <PlusIcon className="size-6" />
            Record
          </button>
          {(activeFilters.length > 0 || showAll) && (
            <div className="flex gap-1">
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
          )}
        </div>
      </div>
      {activeFilters.length > 0 || showAll ? (
        <>
          {viewMode === "list" ? (
            <ListView
              records={tableData}
              activeSearchTerms={activeSearchTerms}
            />
          ) : (
            <GridView
              columns={gridViewColumns}
              data={tableData}
              activeSearchTerms={activeSearchTerms}
            />
          )}
        </>
      ) : (
        <RecentRecordsCard records={recentRecordsData} />
      )}
    </div>
  );
};

export default DataCatalog;
