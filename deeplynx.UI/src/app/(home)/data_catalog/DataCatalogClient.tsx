"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/filter_services.client";

import SavedSearches from "../components/SavedSearches";
import GridView from "../components/GridView";
import ListView from "../components/ListView";
import ProjectDropdown from "../components/ProjectDropdown";
import RecentRecordsCard from "../components/RecentRecordsCard";
import { translations } from "@/app/lib/translations";

import {
  ArrowUturnLeftIcon,
  EyeIcon,
  PlusIcon,
  QueueListIcon,
  TableCellsIcon,
} from "@heroicons/react/24/outline";

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
  const locale = "en";
  const t = translations[locale];

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
  const [totalRecords, setTotalRecords] = useState<number>(
    (initialRecords ?? []).length
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
  const projectIdsToken = useMemo(
    () => projects.map((p) => p.id).join("|"),
    [projects]
  );

  // Update clearAllFilters to respect project selection:
  const clearAllFilters = useCallback(() => {
    setActiveFilters([]);
    setSearchTerm("");

    // Apply project filter even when clearing search filters
    if (selectedProjects.length === 0 || selectedProjects.includes("ALL")) {
      setTableData(initialRecords ?? []);
      setTotalRecords((initialRecords ?? []).length);
    } else {
      const selectedProjectIdsNum = selectedProjects.map((id) => Number(id));
      const filteredData = (initialRecords ?? []).filter((record) =>
        selectedProjectIdsNum.includes(Number(record.projectId))
      );
      setTableData(filteredData);
      setTotalRecords(filteredData.length);
    }
  }, [initialRecords, selectedProjects]);

  const handleSearch = useCallback(
    async (value: string) => {
      const trimmed = value.trim();
      if (!trimmed || activeFilters.some((f) => f.term === trimmed)) return;

      try {
        const newFilter = { id: nextFilterId, term: trimmed };
        const filteredData = await queryRecords(trimmed);

        // Check if "ALL" is selected or no projects selected
        const isAllSelected =
          selectedProjects.length === 0 ||
          selectedProjects.includes("ALL") ||
          selectedProjects.length === projects.length;

        const scopedResults = isAllSelected
          ? filteredData
          : filteredData.filter((r: FileViewerTableRow) => {
              const selectedProjectIdsNum = selectedProjects.map((id) =>
                Number(id)
              );
              return selectedProjectIdsNum.includes(Number(r.projectId));
            });

        setTableData(scopedResults);
        setTotalRecords(scopedResults.length);

        setActiveFilters((prev) => [...prev, newFilter]);
        setNextFilterId((n) => n + 1);
        setSearchTerm("");
        setViewMode("list");
        setShowAll(true);
      } catch (error) {
        console.error("Search error:", error);
      }
    },
    [activeFilters, nextFilterId, selectedProjects]
  );

  // If we arrive with a search term, run it once after session is ready
  useEffect(() => {
    if (!hasLoaded) return;
    if (initialSearchTerm) {
      // run once when session ready
      handleSearch(initialSearchTerm);
    }
  }, [hasLoaded, initialSearchTerm, handleSearch]);

  // Add this useEffect to filter data when selected projects change
  useEffect(() => {
    if (!hasLoaded) return;

    // If there are active filters, let the search handle the filtering
    if (activeFilters.length > 0) return;

    // Filter the initial records based on selected projects
    if (selectedProjects.length === 0 || selectedProjects.includes("ALL")) {
      // Show all records if no specific projects selected
      setTableData(initialRecords ?? []);
      setTotalRecords((initialRecords ?? []).length);
    } else {
      // Filter records by selected project IDs
      const selectedProjectIdsNum = selectedProjects.map((id) => Number(id));
      const filteredData = (initialRecords ?? []).filter((record) =>
        selectedProjectIdsNum.includes(Number(record.projectId))
      );
      setTableData(filteredData);
      setTotalRecords(filteredData.length);
    }
  }, [hasLoaded, selectedProjects, initialRecords, activeFilters.length]);

  // Fetch original records when filters are cleared and projects are selected
  useEffect(() => {
    if (!hasLoaded) return;
    if (activeFilters.length > 0) return;
    if (selectedProjects.length === 0) return;
  }, [
    hasLoaded,
    activeFilters.length,
    selectedProjects.length,
    selectedProjectsToken,
    projectIdsToken,
  ]);

  function renderTags(tags: string) {
    try {
      const parsed: string[] = JSON.parse(tags);
      return parsed
        .filter((t) => t != null)
        .map((t) => (
          <span key={t} className="badge mr-1">
            {t}
          </span>
        ));
    } catch {
      return null;
    }
  }

  const selectedProjectIdsNum = useMemo(
    () => selectedProjects.map((id) => Number(id)),
    [selectedProjects]
  );

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
            className="w-full"
          />
        </div>

        {/* Right: actions */}
        <div className="flex gap-4 pr-4">
          {showAll ? (
            <button
              className="btn btn-outline btn-primary"
              onClick={() => {
                setShowAll(false);
                setViewMode("list");
                clearAllFilters();
              }}
            >
              <ArrowUturnLeftIcon className="h-6 w-6" />
              {t.translations.RECENT_ACTIVITY}
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
              <EyeIcon className="h-6 w-6" />
              {t.translations.EXPLORE_ALL_RECORDS}
            </button>
          )}

          <button className="btn btn-primary text-white">
            <PlusIcon className="h-6 w-6" />
            {t.translations.RECORD}
          </button>

          {(activeFilters.length > 0 || showAll) && (
            <div className="flex gap-1">
              <button
                className={`btn btn-sm ${
                  viewMode === "list" ? "btn-primary" : "btn-ghost"
                }`}
                onClick={() => setViewMode("list")}
              >
                <QueueListIcon className="h-7 w-7" />
              </button>
              <button
                className={`btn btn-sm ${
                  viewMode === "table" ? "btn-primary" : "btn-ghost"
                }`}
                onClick={() => setViewMode("table")}
              >
                <TableCellsIcon className="h-7 w-7" />
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
            selectedProjects={selectedProjectIdsNum}
          />
        ) : (
          <GridView
            columns={[
              { header: "ID", data: "id" },
              {
                header: "Record Name",
                cell: (row) => (
                  <Link
                    href={`/data_catalog/record?recordId=${row.id}&projectId=${row.projectId}`}
                    className="text-base-content font-bold hover:underline"
                  >
                    {row.name}
                  </Link>
                ),
              },
              { header: "Description", data: "description" },
              {
                header: "Class",
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
                header: "Last Edited",
                cell: (row) => row.modifiedAt ?? row.createdAt,
              },
            ]}
            data={tableData}
            activeSearchTerms={activeSearchTerms}
            selectedProjects={selectedProjects}
          />
        )
      ) : (
        <div className="flex w-full gap-8 pl-8">
          <div className="w-2/3">
            <RecentRecordsCard selectedProjects={selectedProjects} />
          </div>
          <div className="w-1/3">
            <SavedSearches />
          </div>
        </div>
      )}
    </div>
  );
}
