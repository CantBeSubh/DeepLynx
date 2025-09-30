"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";

import LargeSearchBar from "@/app/(home)/components/SearchBar";
import { FileViewerTableRow, Tags } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/filter_services.client";
import { getAllRecordsForMultipleProjects } from "@/app/lib/projects_services.client";

import GridView from "../../components/GridView";
import ListView from "../../components/ListView";
import ProjectDropdown from "../../components/ProjectDropdown";

import { useLanguage } from "@/app/contexts/Language";
import { QueueListIcon, TableCellsIcon } from "@heroicons/react/24/outline";

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
  const { hasLoaded } = useProjectSession();

  // Local state
  const [projects] = useState(initialProjects);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(
    initialSelectedProjects
  );
  const [tableData, setTableData] = useState<FileViewerTableRow[]>(
    initialRecords ?? []
  );
  const [searchTerm, setSearchTerm] = useState(initialSearchTerm ?? "");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState(1);
  const [viewMode, setViewMode] = useState<"list" | "table">("list");
  const [showAll, setShowAll] = useState(
    Boolean(initialSearchTerm) || initialSelectedProjects.length > 0
  );

  const activeSearchTerms = useMemo(
    () => activeFilters.map((f) => f.term.toLowerCase()),
    [activeFilters]
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

  const renderTags = (tags: string | null | undefined) => {
    if (!tags) return null;

    try {
      const parsed = JSON.parse(tags);
      const arr = Array.isArray(parsed) ? parsed : [parsed];

      const values = arr.flatMap((item: Tags) => {
        if (item && typeof item === "object") {
          if (typeof item.name === "string") return [item.name];
          return Object.values(item).filter((v) => typeof v === "string");
        }
        return [];
      });

      return (
        <span className="inline-flex flex-wrap gap-2">
          {values.map((v, i) => (
            <span key={`${v}-${i}`} className="badge badge-sm">
              {v}
            </span>
          ))}
        </span>
      );
    } catch {
      return null;
    }
  };

  const selectedProjectIdsNum = useMemo(
    () => selectedProjects.map((id) => Number(id)),
    [selectedProjects]
  );

  if (!hasLoaded) return null;

  return (
    <div>
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2 pb-4">
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
      <div className="flex flex-col gap-4 mb-4 pt-4 pl-8 w-full">
        {/* Top: Search */}
        <div className="flex justify-end pr-10">
          <LargeSearchBar
            placeholder="Search"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onEnter={handleSearch}
            activeFilters={activeFilters}
            onRemoveFilter={(id) =>
              setActiveFilters((prev) => prev.filter((f) => f.id !== id))
            }
            onClearAll={clearAllFilters}
            resultCount={tableData.length}
            showResultsMessage={activeFilters.length > 0}
            className="w-1/4"
          />
        </div>
        <div className="divider my-0"></div>
        {/* Bottom: Actions */}
        <div className="flex items-center justify-between">
          <div className="text-info-content px-4 text-lg">All Records</div>
          <div className="flex gap-1 pr-10">
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
        </div>
        <div className="divider my-0"></div>
      </div>

      {viewMode === "list" ? (
        <ListView
          data={tableData}
          activeSearchTerms={activeSearchTerms}
          selectedProjects={selectedProjectIdsNum}
        />
      ) : (
        <GridView
          columns={[
            { header: "ID", data: "id", sortable: true },
            {
              header: t.translations.RECORD_NAME,
              cell: (row) => (
                <Link
                  href={`/data_catalog/record?recordId=${row.id}&projectId=${row.projectId}`}
                  className="text-info-content font-bold hover:underline"
                >
                  {row.name}
                </Link>
              ),
            },
            {
              header: t.translations.CLASS,
              cell: (row) =>
                row.className ? (
                  <span className="badge text-sm">{row.className}</span>
                ) : null,
            },
            {
              header: "Tags",
              cell: (row) => renderTags(row.tags),
            },
            {
              header: t.translations.LAST_EDIT,
              cell: (row) => row.lastUpdatedAt,
            },
          ]}
          data={tableData}
          activeSearchTerms={activeSearchTerms}
          selectedProjects={selectedProjects}
        />
      )}
    </div>
  );
}
