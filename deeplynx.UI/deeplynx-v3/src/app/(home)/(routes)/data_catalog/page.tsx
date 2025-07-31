"use client";

import { useEffect, useState, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import {
  getAllProjects,
  getAllRecordsForMultipleProjects,
} from "@/app/lib/projects_services";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import ProjectDropdown from "./ProjectDropdown";
import ListView from "./ListView";
import GridView from "./GridView";
import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import RecentRecordsCard from "./RecentRecordsCard";
import SavedSearchesTabs from "../../components/SavedSearches";
import {
  ArrowUturnLeftIcon,
  EyeIcon,
  PlusIcon,
  QueueListIcon,
  TableCellsIcon,
} from "@heroicons/react/24/outline";

const DataCatalogContent = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const fromProject = searchParams.get("fromProject");

  const { project, hasLoaded } = useProjectSession();

  const [tableData, setTableData] = useState<FileViewerTableRow[]>([]);
  const [projects, setProjects] = useState<{ id: string; name: string }[]>([]);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(
    fromProject ? [fromProject] : []
  );
  const [hasMounted, setHasMounted] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState(1);
  const [viewMode, setViewMode] = useState<"list" | "table">("list");
  const [showAll, setShowAll] = useState(false);

  const activeSearchTerms = activeFilters.map((f) => f.term.toLowerCase());

  useEffect(() => {
    setHasMounted(true);
  }, []);

  useEffect(() => {
    const fetchProjects = async () => {
      if (!hasLoaded || project?.projectId === undefined) return;
      try {
        const data = await getAllProjects();
        const mapped = data.map((d: { id: string | number; name: string }) => ({
          id: String(d.id),
          name: d.name,
        }));
        setProjects(mapped);
      } catch (err) {
        console.error("Failed to fetch projects:", err);
      }
    };
    fetchProjects();
  }, [hasLoaded, project?.projectId]);

  useEffect(() => {
    if (!hasLoaded || projects.length === 0) return;

    if (fromProject && !selectedProjects.length) {
      setSelectedProjects([fromProject]);
    }
  }, [fromProject, projects, hasLoaded]);

  useEffect(() => {
    const fetchRecords = async () => {
      if (
        !hasLoaded ||
        selectedProjects.length === 0 ||
        projects.length === 0
      ) {
        return;
      }

      try {
        const projectIdsToQuery = selectedProjects.map((id) => Number(id));
        const records = await getAllRecordsForMultipleProjects(
          projectIdsToQuery
        );
        setTableData(records);
      } catch (error) {
        console.error("Failed to fetch records:", error);
      }
    };

    fetchRecords();
  }, [
    hasLoaded,
    selectedProjects.join(","),
    projects.map((p) => p.id).join(","),
  ]);

  const handleSearch = (value: string) => {
    if (!value.trim() || activeFilters.some((f) => f.term === value.trim()))
      return;
    setActiveFilters([
      ...activeFilters,
      { id: nextFilterId, term: value.trim() },
    ]);
    setNextFilterId(nextFilterId + 1);
    setSearchTerm("");
  };

  const clearAllFilters = () => {
    setActiveFilters([]);
    setSearchTerm("");
  };

  const selectedProjectIds: number[] = selectedProjects.map((id) => Number(id));

  if (!hasMounted) return null;

  return (
    <div>
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-info-content">Data Catalog</h1>
          <ProjectDropdown
            projects={projects}
            onSelectionChange={(ids) => setSelectedProjects(ids)}
            defaultSelected={fromProject ? [fromProject] : undefined}
          />
        </div>
      </div>

      <div className="divider"></div>

      <div className="flex justify-between gap-4 mb-4">
        <LargeSearchBar
          placeholder="Search by name and description..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onEnter={handleSearch}
          activeFilters={activeFilters}
          onRemoveFilter={(id) =>
            setActiveFilters(activeFilters.filter((f) => f.id !== id))
          }
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
                setShowAll(true);
                setViewMode("list");
                clearAllFilters();
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
        viewMode === "list" ? (
          <ListView
            data={tableData}
            activeSearchTerms={activeSearchTerms}
            selectedProjects={selectedProjectIds}
          />
        ) : (
          <GridView
            columns={[
              { header: "ID", data: "id" },
              { header: "Record Name", data: "name" },
              {
                header: "Class",
                cell: (row) =>
                  row.className ? (
                    <span className="badge text-sm">{row.className}</span>
                  ) : null,
              },
              {
                header: "Tags",
                cell: (row) => (
                  <div>
                    {row.tags?.map((t) => (
                      <span key={t.name} className="badge mr-1">
                        {t.name}
                      </span>
                    ))}
                  </div>
                ),
              },
              { header: "Last Edited", data: "modifiedAt" },
            ]}
            data={tableData}
            activeSearchTerms={activeSearchTerms}
            selectedProjects={selectedProjects}
          />
        )
      ) : (
        <div className="flex w-full gap-8">
          <div className="w-2/3">
            <RecentRecordsCard selectedProjects={selectedProjects} />
          </div>
          <div className="w-1/3">
            <SavedSearchesTabs />
          </div>
        </div>
      )}
    </div>
  );
};

const DataCatalog = () => {
  return (
    <Suspense
      fallback={<div className="loading loading-spinner loading-lg"></div>}
    >
      <DataCatalogContent />
    </Suspense>
  );
};

export default DataCatalog;
