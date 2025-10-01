"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/filter_services.client";
import { getAllRecordsForMultipleProjects } from "@/app/lib/projects_services.client";

import ProjectDropdown from "../components/ProjectDropdown";
import RecentRecordsCard from "../components/RecentRecordsCard";
import SavedSearches from "../components/SavedSearches";

import { useLanguage } from "@/app/contexts/Language";
import {
  EyeIcon,
  PlusIcon,
  QueueListIcon,
  TableCellsIcon,
} from "@heroicons/react/24/outline";
import SearchBar from "@/app/(home)/components/SearchBar";
import { fullTextSearch } from "@/app/lib/query_services.client";
import ListView from "../components/ListView";

type Props = {
  initialProjects: { id: string; name: string }[];
  initialSelectedProjects: string[];
  initialSearchTerm: string;
  initialRecords: FileViewerTableRow[];
};

export default function DataCatalogClient({
  initialProjects,
  initialSelectedProjects,
  initialSearchTerm,
  initialRecords,
}: Props) {
  const { t } = useLanguage();

  // Project session (client provider)
  const { hasLoaded, setProject: setProjectSession } = useProjectSession();

  // Local state
  const [projects] = useState(initialProjects);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(
    initialSelectedProjects
  );
  const [tableData, setTableData] = useState<FileViewerTableRow[]>(
    initialRecords ?? []
  );
  const [records, setQueriedRecords] = useState<FileViewerTableRow[]>(initialRecords ?? []);

  const [searchTerm, setSearchTerm] = useState(initialSearchTerm ?? "");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState(1);
  const [viewMode, setViewMode] = useState<"list" | "table">("list");
  const [showAll, setShowAll] = useState(
    Boolean(initialSearchTerm) || initialSelectedProjects.length > 0
  );

  // === memoized “complex” deps ===
  const selectedProjectsToken = useMemo(
    () => selectedProjects.join("|"),
    [selectedProjects]
  );

  // Treat ALL / empty as "all projects"
  const effectiveProjectIds = useMemo(() => {
    const allIds = projects.map((p) => String(p.id));
    if (
      selectedProjects.length === 0 ||
      selectedProjects.includes("ALL") ||
      selectedProjects.length === projects.length
    ) {
      return allIds;
    }
    return selectedProjects.map(String);
  }, [projects, selectedProjects]);

  // Centralized fetch for dropdown-driven changes (no search term here)
  const fetchRecordsForSelection = useCallback(
    async (signal?: AbortSignal) => {
      const idsNum = effectiveProjectIds.map(Number).filter(Number.isFinite);
      if (idsNum.length === 0) {
        setTableData([]);
        return;
      }

      const data = await getAllRecordsForMultipleProjects(idsNum, true, {
        signal,
      });
      setTableData(data);
      // setShowAll(true);
      setViewMode("list");
    },
    [effectiveProjectIds]
  );

  // Clear all now fetches from API for the current scope (not local filtering)
  const clearAllFilters = useCallback(() => {
    setActiveFilters([]);
    setSearchTerm("");
    setQueriedRecords([]);

    const ctrl = new AbortController();
    fetchRecordsForSelection(ctrl.signal).catch((e: FileViewerTableRow) => {
      if (e?.name !== "CanceledError" && e?.name !== "AbortError") {
        console.error("Clear all fetch failed:", e);
      }
    });
  }, [fetchRecordsForSelection]);

  // Search bar (unchanged) — still allowed to query by term and then locally scope
  const handleSearch = useCallback(
    async (value: string) => {
      const trimmed = value.trim();
      if (!trimmed || activeFilters.some((f) => f.term === trimmed)) return;

      const newFilter = { id: nextFilterId, term: trimmed };
      const results = await queryRecords(trimmed);

      const selectedNums = effectiveProjectIds.map(Number);
      const scoped =
        selectedNums.length === projects.length
          ? results
          : results.filter((r: FileViewerTableRow) =>
            selectedNums.includes(Number(r.projectId))
          );

      setTableData(scoped);
      setActiveFilters((prev) => [...prev, newFilter]);
      setNextFilterId((n) => n + 1);
      setSearchTerm("");
      setViewMode("list");
      // setShowAll(true);
    },
    [activeFilters, nextFilterId, effectiveProjectIds, projects.length]
  );

  // Update project session when selectedProjects change
  useEffect(() => {
    if (!hasLoaded) return;
    if (selectedProjects.length > 0) {
      const selectedProject = projects.find(
        (project) => project.id === selectedProjects[0]
      );
      if (selectedProject) {
        setProjectSession({
          projectId: selectedProject.id,
          projectName: selectedProject.name,
        });
      }
    }
  }, [selectedProjects, hasLoaded, projects, setProjectSession]);

  // If we arrive with a search term, run it once after session is ready
  useEffect(() => {
    if (!hasLoaded) return;
    if (initialSearchTerm) {
      handleSearch(initialSearchTerm);
    }
  }, [hasLoaded, initialSearchTerm, handleSearch]);

  // Fetch from API whenever dropdown selection changes and there are no active search filters
  useEffect(() => {
    if (!hasLoaded) return;
    if (activeFilters.length > 0) return; // search takes precedence

    const ctrl = new AbortController();
    fetchRecordsForSelection(ctrl.signal).catch((e: FileViewerTableRow) => {
      if (e?.name !== "CanceledError" && e?.name !== "AbortError") {
        console.error("Fetch on selection change failed:", e);
      }
    });

    return () => ctrl.abort();
  }, [
    hasLoaded,
    activeFilters.length,
    selectedProjectsToken,
    fetchRecordsForSelection,
  ]);


  const handleSubmit = async () => {
    try {

      const data = await fullTextSearch(searchTerm, selectedProjects);
      if (data) {
        setQueriedRecords(data);
      }
    }
    catch (error) {
      console.error("Failed to send query")
    }

  };

  if (!hasLoaded) return null;

  return (
    <div>
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
        <div>
          <h1 className="text-2xl font-bold text-info-content">
            {t.translations.DATA_CATALOG}
          </h1>

          <ProjectDropdown
            projects={projects}
            onSelectionChange={setSelectedProjects}
            defaultSelected={
              initialSelectedProjects.length
                ? initialSelectedProjects
                : undefined
            }
          />
        </div>
      </div>

      <div className="flex justify-between gap-4 mb-4 pt-20 pl-8 w-full box-border">
        {/* Left: Search */}
        <div className="flex flex-col md:w-1/2">
          <SearchBar
            placeholder="Search"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onSubmit={handleSubmit}
            activeFilters={activeFilters}
            onRemoveFilter={(id) =>
              setActiveFilters((prev) => prev.filter((f) => f.id !== id))
            }
            onClearAll={clearAllFilters}
            resultCount={tableData.length}
            showResultsMessage={activeFilters.length > 0}
            className="w-full"
          />
        </div>

        {/* Right: actions */}
        <div className="flex gap-4 pr-4">
          <Link
            href="data_catalog/all_records"
            className="btn btn-outline btn-primary"
            onClick={() => {
              setShowAll(true);
              setViewMode("list");
              clearAllFilters();
            }}
          >
            <EyeIcon className="h-6 w-6" />
            {t.translations.EXPLORE_ALL_RECORDS}
          </Link>
          <button className="btn btn-primary text-white">
            <PlusIcon className="h-6 w-6" />
            {t.translations.RECORD}
          </button>

          {(activeFilters.length > 0 || showAll) && (
            <div className="flex gap-1">
              <button
                className={`btn btn-sm ${viewMode === "list" ? "btn-primary" : "btn-ghost"
                  }`}
                onClick={() => setViewMode("list")}
              >
                <QueueListIcon className="h-7 w-7" />
              </button>
              <button
                className={`btn btn-sm ${viewMode === "table" ? "btn-primary" : "btn-ghost"
                  }`}
                onClick={() => setViewMode("table")}
              >
                <TableCellsIcon className="h-7 w-7" />
              </button>
            </div>
          )}
        </div>
      </div>
      <div className="flex w-full gap-8 p-8">

        {records && records?.length > 0 ? (
          <ListView data={records} />
        ) : (records &&
          <div className="w-2/3">
            <RecentRecordsCard selectedProjects={selectedProjects} />
          </div>
        )}
        {/* <div className="w-1/3">
          <SavedSearches />
        </div> */}
      </div>
    </div>
  );
}
