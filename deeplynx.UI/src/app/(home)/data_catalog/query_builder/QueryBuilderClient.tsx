"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";

import { FileViewerTableRow, Tags } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/filter_services.client";
import { getAllRecordsForMultipleProjects } from "@/app/lib/projects_services.client";

import ProjectDropdown from "../../components/ProjectDropdown";
import { translations } from "@/app/lib/translations";
import AdvancedSearchBar from "../../components/AdvancedSearchBar";
import { PlusIcon, TrashIcon, XMarkIcon } from "@heroicons/react/24/outline";

type Props = {
  initialProjects: { id: string; name: string }[];
  initialSelectedProjects: string[];
  initialSearchTerm: string;
  initialRecords: FileViewerTableRow[];
  connectors?: string[];
  filters?: string[];
  operators?: string[];
  values?: string[];
};

export type Condition = {
  id: string;
  connector?: string;
  filter?: string;
  operator?: string;
  value?: string;
};

const newId = () => Math.random().toString(36).slice(2, 10);
const emptyRow = (): Condition => ({ id: newId(), connector: "", filter: "", operator: "", value: "" });

export default function QueryBuilderClient({
  initialProjects,
  initialSelectedProjects,
  initialSearchTerm,
  initialRecords,
  connectors = ["AND", "OR", "NOT"],
  filters = ["Time Range", "Storage Destination Type", "Class", "Tag", "Project", "Original Data ID", "Data Source", "Property Field"],
  operators = ["=", "<", ">", "LIKE"],
  values = ["ClassOne", "ClassTwo", "ClassThree"]
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
  const [searchTerm, setSearchTerm] = useState(initialSearchTerm ?? "");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState(1);
  const [viewMode, setViewMode] = useState<"list" | "table">("list");
  const [rows, setRows] = useState<Condition[]>([emptyRow()]);


  const addRow = () => setRows((r) => [...r, emptyRow()]);
  const removeRow = (id: string) => setRows((r) => (r.length > 1 ? r.filter((x) => x.id !== id) : r));
  const updateRow = (id: string, patch: Partial<Condition>) =>
    setRows((r) => r.map((row) => (row.id === id ? { ...row, ...patch } : row)));
  const reset = () => setRows([emptyRow()]);


  // strip id for preview
  const preview = rows.map(({ id, ...rest }) => rest);

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
      <div className=" gap-4 mb-4 pt-4 pl-8 w-full">
        <div className="text-info-content px-4 text-xl"> {t.translations.ADDITIONAL_FILTERS}</div>

        {/* Top: Search */}
        <div className="shadow-md rounded-md py-4">
          <div className="flex w-full justify-left p-8">
            <AdvancedSearchBar
              placeholder="Enter Search Term"
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
              className="w-full max-w-3xl"
            />
          </div>
          <div className="text-info-content pl-8 text-lg">
            {t.translations.SELECT_FILTERS}
          </div>
          <div>
            <div>
              {rows.map((row, idx) => (
                <div key={row.id} className="card">
                  <div className="card-body grid grid-cols-1 md:grid-cols-5 gap-3">
                    <select
                      className="select select-sm select-bordered w-full"
                      value={row.connector}
                      onChange={(e) => updateRow(row.id, { connector: e.target.value })}
                    >
                      <option value="" disabled>
                        {t.translations.CONNECTOR}
                      </option>
                      {connectors.map((opt) => (
                        <option key={opt} value={opt}>
                          {opt}
                        </option>
                      ))}
                    </select>


                    <select
                      className="select select-sm select-bordered w-full"
                      value={row.filter}
                      onChange={(e) => updateRow(row.id, { filter: e.target.value })}
                    >
                      <option value="" disabled>
                        {t.translations.FILTER}
                      </option>
                      {filters.map((opt) => (
                        <option key={opt} value={opt}>
                          {opt}
                        </option>
                      ))}
                    </select>
                    <select
                      className="select select-sm select-bordered w-full"
                      value={row.operator}
                      onChange={(e) => updateRow(row.id, { operator: e.target.value })}
                    >
                      <option value="" disabled>
                        {t.translations.OPERATOR}
                      </option>
                      {operators.map((opt) => (
                        <option key={opt} value={opt}>
                          {opt}
                        </option>
                      ))}
                    </select>
                    <select
                      className="select select-sm select-bordered w-full"
                      value={row.value}
                      onChange={(e) => updateRow(row.id, { value: e.target.value })}
                    >
                      <option value="" disabled>
                        {t.translations.VALUE}
                      </option>
                      {values.map((opt) => (
                        <option key={opt} value={opt}>
                          {opt}
                        </option>
                      ))}
                    </select>
                    <div className="flex">
                      <button
                        type="button"
                        className="btn btn-outline btn-error btn-sm"
                        onClick={() => removeRow(row.id)}
                      >
                        <TrashIcon className="w-4 h-4" />
                      </button>
                    </div>
                    {/* Buttons under the dropdowns, aligned to the grid edges, only on the last row */}
                    {idx === rows.length - 1 && (
                      <div className="md:col-span-5 flex items-center justify-between pt-2 pr-65">
                        <button
                          type="button"
                          className="btn btn-outline btn-sm"
                          onClick={addRow}
                        >
                          <PlusIcon className="w-4 h-4" /> {t.translations.FILTER}
                        </button>
                        <button onClick={reset} className="btn btn-error btn-outline btn-sm">
                          <XMarkIcon className="w-4 h-4" />{t.translations.DELETE_ALL_FILTERS}
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
