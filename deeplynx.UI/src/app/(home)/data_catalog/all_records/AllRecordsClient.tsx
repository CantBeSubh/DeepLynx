"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";

import SearchBar from "@/app/(home)/components/SearchBar";
import { FileViewerTableRow, Tags } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/client_service/filter_services.client";

import GridView from "../../components/GridView";
import ListView from "../../components/ListView";
import ProjectDropdown from "../../components/ProjectDropdown";

import { useLanguage } from "@/app/contexts/Language";
import {
  AdjustmentsHorizontalIcon,
  ArrowUturnLeftIcon,
  EyeIcon,
  QueueListIcon,
  TableCellsIcon,
  ChevronDownIcon,
} from "@heroicons/react/24/outline";
import { fullTextSearch } from "@/app/lib/query_services.client";

/* ----------------------------- Types & utils ----------------------------- */

type Props = {
  initialProjects: { id: string; name: string }[];
  initialSelectedProjects: string[];
  initialSearchTerm: string;
  initialRecords: FileViewerTableRow[];
};

type ColumnKey = "id" | "name" | "class" | "tags" | "lastEdit";
type ColumnDef<Row> = {
  key: ColumnKey;
  header: string;
  data?: keyof Row;
  sortable?: boolean;
  cell?: (row: Row) => React.ReactNode;
};

function useClickOutside(
  ref: React.RefObject<HTMLElement | null>,
  onAway: () => void
) {
  useEffect(() => {
    const h = (e: MouseEvent) => {
      if (!ref.current) return;
      if (!ref.current.contains(e.target as Node)) onAway();
    };
    document.addEventListener("mousedown", h);
    return () => document.removeEventListener("mousedown", h);
  }, [onAway, ref]);
}

function toggle<T>(arr: T[], val: T) {
  return arr.includes(val) ? arr.filter((x) => x !== val) : [...arr, val];
}

/* -------------------------------- Component ------------------------------ */

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

      // const data = await getAllRecordsForMultipleProjects(idsNum, true, {
      //   signal,
      // });
      // setTableData(data);
      // setViewMode("list");
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

  // eslint-disable-next-line react-hooks/exhaustive-deps
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

  /* ------------------------ Column visibility wiring ------------------------ */

  // Define all possible columns with stable keys
  const ALL_COLUMNS: ColumnDef<FileViewerTableRow>[] = useMemo(
    () => [
      { key: "id", header: "ID", data: "id", sortable: true },
      {
        key: "name",
        header: t.translations.RECORD_NAME,
        cell: (row) => (
          <Link
            href={`/record?recordId=${row.id}&projectId=${row.projectId}`}
            className="text-info-content font-bold hover:underline"
          >
            {row.name}
          </Link>
        ),
      },
      {
        key: "class",
        header: t.translations.CLASS,
        cell: (row) =>
          row.className ? (
            <span className="badge text-sm">{row.className}</span>
          ) : null,
      },
      {
        key: "tags",
        header: "Tags",
        cell: (row) => renderTags(row.tags),
      },
      {
        key: "lastEdit",
        header: t.translations.LAST_EDIT,
        cell: (row) => row.lastUpdatedAt,
      },
    ],
    [t.translations, renderTags]
  );

  // Visible column keys
  const [visibleCols, setVisibleCols] = useState<ColumnKey[]>(
    ALL_COLUMNS.map((c) => c.key) // default: all visible
  );

  const filteredColumns = useMemo(
    () => ALL_COLUMNS.filter((c) => visibleCols.includes(c.key)),
    [ALL_COLUMNS, visibleCols]
  );

  // Strip "key" before passing to GridView if it doesn’t expect it
  const gridColumns = useMemo(
    () => filteredColumns.map(({ key, ...rest }) => rest),
    [filteredColumns]
  );

  // Dropdown UI state
  const [open, setOpen] = useState(false);
  const ddRef = useRef<HTMLDivElement | null>(null);
  useClickOutside(ddRef, () => setOpen(false));

  // Select All helpers
  const allSelected = visibleCols.length === ALL_COLUMNS.length;
  const someSelected = visibleCols.length > 0 && !allSelected;

  const handleToggleOne = (k: ColumnKey) =>
    setVisibleCols((prev) => {
      const next = toggle(prev, k);
      return next.length === 0 ? prev : next; // require at least one
    });

  const handleToggleAll = () =>
    setVisibleCols((prev) =>
      prev.length === ALL_COLUMNS.length
        ? [ALL_COLUMNS[0].key]
        : ALL_COLUMNS.map((c) => c.key)
    );

  /* --------------------------------- Search -------------------------------- */

  const handleSubmit = async () => {
    try {
      // const data = await fullTextSearch(searchTerm, selectedProjects);
      // if (data) setTableData(data);
    } catch (error) {
      console.error("Failed to send query", error);
    }
  };

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
            className="w-1/4"
          />
        </div>

        <div className="divider my-0"></div>

        {/* Bottom: Actions */}
        <div className="flex items-center justify-between">
          <div className="text-info-content px-4 text-lg">All Records</div>

          <div className="flex gap-2 pr-10 items-center">
            <div className="pr-2">
              <Link
                href="/data_catalog"
                className="btn btn-sm btn-outline btn-primary"
                onClick={() => {
                  setViewMode("list");
                  clearAllFilters();
                }}
              >
                <ArrowUturnLeftIcon className="h-7 w-6" />
                {t.translations.DATA_CATALOG}
              </Link>
            </div>

            <button
              className={`btn btn-sm ${
                viewMode === "list" ? "btn-primary" : "btn-ghost"
              }`}
              onClick={() => setViewMode("list")}
              title="List view"
            >
              <QueueListIcon className="h-7 w-7" />
            </button>

            <button
              className={`btn btn-sm ${
                viewMode === "table" ? "btn-primary" : "btn-ghost"
              }`}
              onClick={() => setViewMode("table")}
              title="Table view"
            >
              <TableCellsIcon className="h-7 w-7" />
            </button>

            {/* Column visibility dropdown */}
            {viewMode.includes("table") && (
              <div className="relative" ref={ddRef}>
                <button
                  type="button"
                  onClick={() => setOpen((o) => !o)}
                  className="btn btn-sm btn-outline btn-primary inline-flex items-center gap-2"
                  aria-haspopup="menu"
                  aria-expanded={open}
                >
                  <EyeIcon className="h-5 w-5" />
                  <span>Column Visibility</span>
                  <span className="hidden md:inline-block text-xs bg-base-200 px-2 py-0.5 rounded">
                    {visibleCols.length - 1}/{ALL_COLUMNS.length - 1}
                  </span>
                  <ChevronDownIcon
                    className={`h-4 w-4 transition-transform ${
                      open ? "rotate-180" : ""
                    }`}
                  />
                </button>

                {open && (
                  <div
                    role="menu"
                    className="absolute right-0 z-50 mt-2 w-60 rounded-lg border border-gray-200 bg-white p-2 shadow-lg"
                  >
                    <label className="flex items-center gap-2 rounded px-2 py-1.5 hover:bg-gray-50 cursor-pointer">
                      {/* Indeterminate via ref callback */}
                      <input
                        type="checkbox"
                        className="h-4 w-4"
                        checked={allSelected}
                        ref={(el) => {
                          if (el) el.indeterminate = someSelected;
                        }}
                        onChange={handleToggleAll}
                      />
                      <span className="text-sm text-gray-900">Select all</span>
                    </label>

                    <div className="my-1 h-px bg-gray-200" />

                    {ALL_COLUMNS.filter((c) => c.key !== "id").map((c) => (
                      <label
                        key={c.key}
                        className="flex items-center gap-2 rounded px-2 py-1.5 hover:bg-gray-50 cursor-pointer"
                      >
                        <input
                          type="checkbox"
                          className="h-4 w-4"
                          checked={visibleCols.includes(c.key)}
                          onChange={() => handleToggleOne(c.key)}
                        />
                        <span className="text-sm text-gray-900">
                          {c.header}
                        </span>
                      </label>
                    ))}
                  </div>
                )}
              </div>
            )}
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
          columns={gridColumns}
          data={tableData}
          activeSearchTerms={activeSearchTerms}
          selectedProjects={selectedProjects}
        />
      )}
    </div>
  );
}
