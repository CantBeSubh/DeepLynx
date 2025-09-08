"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";

import { ClassResponseDto, DataSourceResponseDto, TagResponseDto, CustomQueryRequestDto, HistoricalRecordResponseDto, FileViewerTableRow } from "@/app/(home)/types/types";
import ProjectDropdown from "../../components/ProjectDropdown";
import { translations } from "@/app/lib/translations";
import AdvancedSearchBar from "../../components/AdvancedSearchBar";
import { PlusCircleIcon, PlusIcon, StarIcon, TrashIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { DatePicker } from "../../components/DatePicker";
import { getClassesForProjects } from "@/app/lib/class_services client";
import { getDataSourcesForProjects } from "@/app/lib/data_source_services.client";
import { getTagsForProjects } from "@/app/lib/tag_services.client";
import { queryBuilder } from "@/app/lib/query_services.client";
import ListView from "../../components/ListView";


type Props = {
  initialProjects: { id: string; name: string }[];
  initialSelectedProjects: string[];
  initialSearchTerm: string;
  connectors?: string[];
  filters?: { name: string; value: string }[];
  operators?: string[];
  values?: string[];
  queriedRecords: FileViewerTableRow[];
};

export type Query = {
  id: string;
  query: CustomQueryRequestDto;
};

const newId = () => Math.random().toString(36).slice(2, 10);
const emptyRow = (): Query => ({ id: newId(), query: { connector: "", filter: "", operator: "", value: "" } });

export default function QueryBuilderClient({
  initialProjects,
  initialSelectedProjects,
  initialSearchTerm,
  connectors = ["AND", "OR", "NOT"],
  filters = [{ name: "Class", value: "ClassName" }, { name: "Tag", value: "Tags" }, { name: "Original Data ID", value: "OriginalId" }, { name: "Data Source", value: "DataSourceName" }, { name: "Properties", value: "Properties" }],
  operators = ["=", "<", ">", "LIKE", "KEY_VALUE"],
  values = ["ClassOne", "ClassTwo", "ClassThree"],
  queriedRecords
}: Props) {

  const locale = "en";
  const t = translations[locale];

  const [projects] = useState(initialProjects);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(initialSelectedProjects);
  const [classes, setClasses] = useState<ClassResponseDto[]>([]);
  const [datasources, setDataSources] = useState<DataSourceResponseDto[]>([]);
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isLoadingClasses, setIsLoadingClasses] = useState(false);
  const [isLoadingDataSources, setIsLoadingDataSources] = useState(false);
  const [isLoadingTags, setIsLoadingTags] = useState(false);
  const [records, setQueriedRecords] = useState<FileViewerTableRow[] | null>(queriedRecords);


  const [searchTerm, setSearchTerm] = useState(initialSearchTerm ?? "");
  const [activeFilters, setActiveFilters] = useState<Array<{ id: number; term: string }>>([]);
  const [rows, setRows] = useState<Query[]>([emptyRow()]);
  const { project, hasLoaded } = useProjectSession();


  const currentProjectId = useMemo<string>(() => {
    const firstProjectId = projects.length > 0 ? String(projects[0].id) : "";
    if (
      selectedProjects.length === 0 ||
      selectedProjects.includes("ALL") ||
      selectedProjects.length === projects.length
    ) {
      return firstProjectId;
    }
    return String(selectedProjects[0]);
  }, [projects, selectedProjects]);

  const addRow = () => setRows((r) => [...r, emptyRow()]);
  const removeRow = (id: string) => setRows((r) => (r.length > 1 ? r.filter((x) => x.id !== id) : r));
  const updateRow = (id: string, patch: Partial<Query>) =>
    setRows((r) => r.map((row) => (row.id === id ? { ...row, ...patch } : row)));
  const reset = () => {
    setRows([emptyRow()]);
    setQueriedRecords(null);
  }




  useEffect(() => {
    async function loadClasses() {
      try {
        setIsLoadingClasses(true);
        const data = await getClassesForProjects(currentProjectId, selectedProjects);
        setClasses(data);
      } catch (error) {
        console.error("Failed to fetch classes:", error);
        setClasses([]);
      } finally {
        setIsLoadingClasses(false);
        setLoading(false);
      }
    }

    if (hasLoaded && currentProjectId) {
      loadClasses();
    }
  }, [hasLoaded, currentProjectId]);

  useEffect(() => {
    async function loadDataSources() {
      try {
        setIsLoadingDataSources(true);
        const data = await getDataSourcesForProjects(currentProjectId, selectedProjects);
        setDataSources(data);
      } catch (error) {
        console.error("Failed to fetch classes:", error);
        setDataSources([]);
      } finally {
        setIsLoadingDataSources(false);
        setLoading(false);
      }
    }
    if (hasLoaded && currentProjectId) {
      loadDataSources();
    }
  }, [hasLoaded, currentProjectId]);

  useEffect(() => {
    async function loadTags() {
      try {
        setIsLoadingTags(true);
        const data = await getTagsForProjects(currentProjectId, selectedProjects);
        setTags(data);
      } catch (error) {
        console.error("Failed to fetch classes:", error);
        setTags([]);
      } finally {
        setIsLoadingTags(false);
        setLoading(false);
      }
    }

    if (hasLoaded && currentProjectId) {
      loadTags();
    }
  }, [hasLoaded, currentProjectId]);

  const handleSubmit = async () => {
    try {
      const queryDtos = rows.map(r => r.query);
      let data = await queryBuilder(queryDtos, searchTerm);
      console.log(data)
      if (data) {
        setQueriedRecords(data);
      }
    } catch (error) {
      console.error("Failed to send query")
    }

  };

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
      <div className="gap-4 mb-4 pt-4 pl-8 w-full">
        <div className="text-info-content p-4 text-xl"> {t.translations.ADDITIONAL_FILTERS}</div>
        <div className="shadow-md rounded-md mr-6">
          <div className="flex justify-between p-8 gap-7 ">
            {/* Advanced Search */}
            <div className="rounded-md py-4">
              {/* Full text search */}
              <div className="flex w-full justify-left p-8">
                <AdvancedSearchBar
                  placeholder="Enter Search Term"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  activeFilters={activeFilters}
                  onRemoveFilter={(id) =>
                    setActiveFilters((prev) => prev.filter((f) => f.id !== id))
                  }
                  showResultsMessage={activeFilters.length > 0}
                  className="w-full max-w-3xl"
                />
              </div>
              {/* Query Builder */}
              <div className="text-info-content pl-8 text-lg">
                {t.translations.SELECT_FILTERS}
              </div>
              <div>
                <div>
                  {rows.map((row, idx) => (
                    <div key={row.id} className="card">
                      <div className="card-body grid grid-cols-1 sm:grid-cols-6 gap-2 w-full">
                        {/* Connector */}
                        <select
                          className="select select-sm select-bordered w-full"
                          value={row.query.connector ?? ""}
                          onChange={(e) =>
                            updateRow(row.id, {
                              query: {
                                ...row.query,
                                connector: e.target.value,
                              },
                            })
                          }
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

                        {/* Filter */}
                        <select
                          className="select select-sm select-bordered w-full"
                          value={row.query.filter}
                          onChange={async (e) => {
                            const value = e.target.value;
                            updateRow(row.id, {
                              query: {
                                ...row.query,
                                filter: e.target.value,
                              },
                            });

                            if (value === "ClassName") {
                              getClassesForProjects(currentProjectId, selectedProjects)
                                .then(setClasses)
                                .catch((err: Error) => console.error("Failed to fetch classes:", err));
                            }
                            if (value === "DataSourceName") {
                              getDataSourcesForProjects(currentProjectId, selectedProjects)
                                .then(setDataSources)
                                .catch((err: Error) => console.error("Failed to fetch datasources:", err));
                            }
                            if (value === "Tags") {
                              getTagsForProjects(currentProjectId, selectedProjects)
                                .then(setTags)
                                .catch((err) => console.error("Failed to fetch tags:", err));
                            }
                          }}
                        >
                          <option value="" disabled>
                            {t.translations.FILTER}
                          </option>
                          {filters.map((opt) => (
                            <option key={opt.name} value={opt.value}>
                              {opt.name}
                            </option>
                          ))}
                        </select>

                        {/* Operator */}
                        <select
                          className="select select-sm select-bordered w-full"
                          value={
                            row.query.operator
                          }
                          onChange={(e) =>
                            updateRow(row.id, {
                              query: {
                                ...row.query,
                                operator: e.target.value,
                              },
                            })
                          }

                        >
                          <option value="" disabled>
                            {t.translations.OPERATOR}
                          </option>

                          {operators
                            // filter logic
                            .filter((opt) => {
                              if (row.query.filter === "Properties") {
                                return opt === "KEY_VALUE";
                              }
                              if (row.query.filter === "Time Range") {
                                return opt === "<" || opt === ">" || opt === '='; // only show < or >
                              }
                              if (row.query.filter === "ClassName" || row.query.filter === "OriginalId" || row.query.filter === "DataSourceName" || row.query.filter === "Tags") {
                                return opt !== "<" && opt !== ">" && opt !== 'KEY'; // hide < and >
                              }
                              return true; // otherwise allow all
                            })
                            .map((opt) => (
                              <option key={opt} value={opt}>
                                {opt}
                              </option>
                            ))}
                        </select>

                        {/* Text input for Property Field; select for others (except Time Range) */}
                        {/* Value */}
                        {row.query.filter !== "Time Range" && (
                          (row.query.filter === "Properties" || row.query.filter === "OriginalId") ? (
                            <input
                              type="text"
                              className="input input-sm input-bordered w-full"
                              value={
                                row.query.filter === "Properties"
                                  ? (row.query.json ?? "")
                                  : (row.query.value ?? "")
                              }
                              onChange={(e) => updateRow(row.id, {
                                query: {
                                  ...row.query,
                                  ...(row.query.filter === "Properties"
                                    ? { json: e.target.value }
                                    : { value: e.target.value }),
                                },
                              })}
                              placeholder={t.translations.VALUE}
                            />
                          ) : (
                            <select
                              className="select select-sm select-bordered w-full"
                              value={row.query.value}
                              onChange={(e) => updateRow(row.id, {
                                query: {
                                  ...row.query,
                                  value: e.target.value,
                                },
                              })}
                              disabled={
                                (row.query.filter === "ClassName" && isLoadingClasses) ||
                                (row.query.filter === "DataSourceName" && isLoadingDataSources) ||
                                (row.query.filter === "Tags" && isLoadingTags)
                              }
                            >
                              <option value="" disabled>{t.translations.VALUE}</option>

                              {row.query.filter === "ClassName" ? (
                                classes.length ? (
                                  classes.map((opt) => (
                                    <option key={opt.id} value={opt.name}>
                                      {opt.name}
                                    </option>
                                  ))
                                ) : (
                                  <option disabled value="">
                                    {isLoadingClasses ? "Loading classes..." : "No classes found"}
                                  </option>
                                )
                              ) : row.query.filter === "DataSourceName" ? (
                                datasources.length ? (
                                  datasources.map((opt) => (
                                    <option key={opt.id} value={opt.name}>
                                      {opt.name}
                                    </option>
                                  ))
                                ) : (
                                  <option disabled value="">
                                    {isLoadingDataSources ? "Loading datasources..." : "No datasources found"}
                                  </option>
                                )
                              ) : row.query.filter === "Tags" ? (
                                tags.length ? (
                                  tags.map((opt) => (
                                    <option key={opt.id} value={opt.name}>
                                      {opt.name}
                                    </option>
                                  ))
                                ) : (
                                  <option disabled value="">
                                    {isLoadingTags ? "Loading tags..." : "No tags found"}
                                  </option>
                                )
                              ) : (
                                values.map((opt) => (
                                  <option key={opt} value={opt}>
                                    {opt}
                                  </option>
                                ))
                              )}
                            </select>
                          )
                        )}

                        {/* Time Range*/}
                        {row.query.filter === "Time Range" ? (
                          <DatePicker
                            row={row}
                            onChange={(dateTime: string) =>
                              updateRow(row.id, {
                                query: {
                                  ...row.query,
                                  connector: dateTime,
                                },
                              }) // store datetime in row.value
                            }
                          />
                        ) : null}

                        <div className="w-full"></div>

                        <div className="w-full">
                          <button
                            type="button"
                            className="btn btn-outline btn-error btn-sm"
                            onClick={() => removeRow(row.id)}
                          >
                            <TrashIcon className="w-4 h-4" />
                          </button>
                        </div>

                        {idx === rows.length - 1 && (
                          <div className="md:col-span-6 flex items-center justify-between pt-2 pr-15">
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
            {/* Other search controls */}
            <div className="max-h-60 shadow-md w-1/4 text-info-content rounded-lg flex flex-col">
              <div className="p-6 text-sm">Other Search Controls and Options</div>

              {/* Saved searches */}
              <div className="flex items-center justify-between text-xs px-6">
                <span className="hidden sm:inline">Add to saved searches</span>
                <button onClick={reset} className="btn btn-ghost btn-sm">
                  <PlusCircleIcon className="w-4 h-4" />
                </button>
              </div>

              {/* Favorites */}
              <div className="flex items-center justify-between text-xs px-6">
                <span className="hidden sm:inline">Add to favorites searches</span>
                <button onClick={reset} className="btn btn-ghost btn-sm">
                  <StarIcon className="w-4 h-4" />
                </button>
              </div>
            </div>


          </div>
          {/* Submit search */}
          <div className="grid justify-items-end p-4">
            <button onClick={handleSubmit} className="btn btn-primary btn-sm">Search Records
            </button>
          </div>
        </div>
      </div>
      {records &&
        <ListView
          data={records}
        />
      }
    </div>
  );
}
