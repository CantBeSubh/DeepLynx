"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import {
  getAllProjects,
  getAllRecordsForMultipleProjects,
} from "@/app/lib/projects_services";
import {
  ArrowUturnLeftIcon,
  EyeIcon,
  PlusIcon,
  QueueListIcon,
  TableCellsIcon,
} from "@heroicons/react/24/outline";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import SavedSearchesTabs from "../../components/SavedSearches";
import ExpandableTagsCell from "./ExpandableTagCell";
import GridView from "./GridView";
import ListView from "./ListView";
import ProjectDropdown from "./ProjectDropdown";
import RecentRecordsCard from "./RecentRecordsCard";

const DataCatalog = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const fromProject = searchParams.get("fromProject");
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
  const [projects, setProjects] = useState<{ id: string; name: string }[]>([]);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(
    fromProject ? [fromProject] : []
  );

  // Effect to set project name from localStorage and mark the component as mounted
  useEffect(() => {
    setHasMounted(true);
    const storedName = localStorage.getItem("selectedProjectName");
    if (storedName) setProjectName(storedName);

    if (fromProject === "ALL") {
      setSelectedProjects(projects.map((p) => p.id));
    } else if (fromProject) {
      setSelectedProjects([fromProject]);
    }
  }, [fromProject, projects]);

  useEffect(() => {
    const fetchProjects = async () => {
      if (!hasLoaded || !project?.projectId === undefined) return;

      try {
        const data = await getAllProjects();
        setProjects(
          data.map((d: { id: string; name: string }) => ({
            id: d.id,
            name: d.name,
          }))
        );
      } catch (error) {
        console.error("Failed to fetch records:", error);
      }
    };
    fetchProjects();
  }, [project?.projectId, hasLoaded]);

  useEffect(() => {
    if (!hasLoaded || projects.length === 0) return;

    if (fromProject === "ALL") {
      const allProjectIds = projects
        .map((p) => p.id)
        .filter((id) => id !== undefined);
      setSelectedProjects(allProjectIds);
    } else if (fromProject && !selectedProjects.length) {
      setSelectedProjects([fromProject]);
    }
  }, [fromProject, projects, hasLoaded]);

  useEffect(() => {
    // Only run this if we haven't already selected projects
    if (projects.length > 0 && selectedProjects.length === 0 && hasLoaded) {
      const allProjectIds = projects
        .map((p) => p.id)
        .filter((id) => id !== undefined);
      setSelectedProjects(allProjectIds);
    }
  }, [projects, selectedProjects, hasLoaded]);

  useEffect(() => {
    const fetchRecords = async () => {
      if (
        !hasLoaded ||
        selectedProjects.length === 0 ||
        projects.length === 0 ||
        project?.projectId === undefined
      ) {
        console.log("Skipping fetch - condition not met:", {
          hasLoaded,
          selectedProjects,
          projectsLoaded: projects.length,
        });
        return;
      }

      try {
        const projectIdsToQuery = selectedProjects.map((id) => Number(id));
        const records = await getAllRecordsForMultipleProjects(
          projectIdsToQuery
        );
        console.log("Setting tableData:", records);
        setTableData(records);
      } catch (error) {
        console.error("Failed to fetch records:", error);
      }
    };

    fetchRecords();
  }, [
    hasLoaded,
    JSON.stringify(selectedProjects),
    JSON.stringify(projects.map((p) => p.id)),
  ]);

  useEffect(() => {
    console.log("Selected projects updated:", selectedProjects);
  }, [selectedProjects]);

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

  const selectedProjectIds: number[] = selectedProjects.includes("ALL")
    ? projects.map((p) => Number(p.id))
    : selectedProjects.map((id) => Number(id));

  // If the component has not mounted yet, return null to avoid rendering
  if (!hasMounted) {
    return null;
  }

  return (
    <div>
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-info-content">Data Catalog</h1>
          <ProjectDropdown
            projects={projects}
            onSelectionChange={(ids: string[]) => setSelectedProjects(ids)}
            defaultSelected={fromProject ? [fromProject] : undefined}
          />
        </div>
      </div>
      <div className="divider"></div>
      <div className="flex justify-between gap-4 mb-4">
        <LargeSearchBar
          placeholder="Seach by name and description ..."
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
              data={tableData}
              activeSearchTerms={activeSearchTerms}
              selectedProjects={selectedProjectIds}
            />
          ) : (
            <GridView
              columns={gridViewColumns}
              data={tableData}
              activeSearchTerms={activeSearchTerms}
              selectedProjects={selectedProjects}
            />
          )}
        </>
      ) : (
        <div className="flex w-full gap-8">
          <div className="w-2/3">
            <RecentRecordsCard selectedProjects={selectedProjects} />
          </div>
          <div className="w-1/3">
            <SavedSearchesTabs className="" />
          </div>
        </div>
      )}
    </div>
  );
};

export default DataCatalog;
