"use client";

import React, { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/filter_services"; // if this calls HTTP, it's fine client-side

import SavedSearches from "../../components/SavedSearches";
import GridView from "./GridView";
import ListView from "./ListView";
import ProjectDropdown from "./ProjectDropdown";
import RecentRecordsCard from "./RecentRecordsCard"; // make sure this is client or has a client wrapper
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
  const router = useRouter();

  // Project session (client provider)
  const { project, hasLoaded } = useProjectSession();

  // Local state
  const [projects, setProjects] = useState(initialProjects);
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

  // If we arrive with a search term, run it once after session is ready
  useEffect(() => {
    if (!hasLoaded) return;
    if (initialSearchTerm) {
      handleSearch(initialSearchTerm);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hasLoaded]);

  // Fetch original records when filters are cleared and projects are selected
  useEffect(() => {
    if (!hasLoaded) return;
    if (activeFilters.length > 0) return;
    if (selectedProjects.length === 0) return;
  }, [hasLoaded, activeFilters.length, selectedProjects.join(",")]);

  async function handleSearch(value: string) {
    const trimmed = value.trim();
    if (!trimmed || activeFilters.some((f) => f.term === trimmed)) return;

    try {
      const newFilter = { id: nextFilterId, term: trimmed };
      const filteredData = await queryRecords(trimmed); // client-side API call

      const selectedProjectIdsNum = selectedProjects.map((id) => Number(id));
      const scopedResults = selectedProjectIdsNum.length
        ? filteredData.filter((r: FileViewerTableRow) =>
            selectedProjectIdsNum.includes(Number(r.projectId))
          )
        : filteredData;

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
  }

  function clearAllFilters() {
    setActiveFilters([]);
    setSearchTerm("");
    setTableData(initialRecords ?? []);
    setTotalRecords((initialRecords ?? []).length);
  }

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

  const selectedProjectIdsNum = selectedProjects.map((id) => Number(id));

  // Guard on session provider readiness (optional depending on your app)
  if (!hasLoaded) return null;

  return (
    <div>
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
        <div>
          <h1 className="text-2xl font-bold text-info-content">Data Catalog</h1>

          <ProjectDropdown
            projects={projects}
            onSelectionChange={(ids) => setSelectedProjects(ids)}
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
              <EyeIcon className="h-6 w-6" />
              Explore All Records
            </button>
          )}

          <button className="btn btn-primary text-white">
            <PlusIcon className="h-6 w-6" />
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
