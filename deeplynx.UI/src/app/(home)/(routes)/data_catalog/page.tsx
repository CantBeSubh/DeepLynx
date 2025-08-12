"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { queryRecords } from "@/app/lib/filter_services";
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
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import SavedSearchesTabs from "../../components/SavedSearches";
import GridView from "./GridView";
import ListView from "./ListView";
import ProjectDropdown from "./ProjectDropdown";
import RecentRecordsCard from "./RecentRecordsCard";
import { translations } from "@/app/lib/translations";
import React from "react";

const DataCatalogContent = () => {
  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];
  const router = useRouter();
  const searchParams = useSearchParams();
  const fromProject = searchParams.get("fromProject");
  const initialSearch = searchParams.get("search");

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

    if (initialSearch && !hasMounted && hasLoaded) {
      handleSearch(initialSearch);
    }

    if (initialSearch) {
      setSearchTerm(initialSearch);
    }
  }, [initialSearch, hasMounted, hasLoaded]);

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
      // Only fetch original data if there are no active filters
      if (activeFilters.length > 0) {
        return;
      }

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
    activeFilters.length, // Add this dependency so it refetches when filters are cleared
  ]);

  const handleSearch = async (value: string) => {
    const trimmed = value.trim();
    if (!trimmed || activeFilters.some((f) => f.term === trimmed)) {
      return;
    }

    try {
      const newFilters = [
        ...activeFilters,
        { id: nextFilterId, term: trimmed },
      ];
      const allSearchTerm = newFilters.map((f) => f.term);
      const filteredData = await queryRecords(value);

      setTableData(filteredData);
      setActiveFilters([...activeFilters, { id: nextFilterId, term: trimmed }]);
      setNextFilterId(nextFilterId + 1);
      setSearchTerm("");
      setViewMode("list");
      setShowAll(true);
    } catch (error) {
      console.error("Search error:", error);
    }
  };

  const clearAllFilters = () => {
    setActiveFilters([]);
    setSearchTerm("");
  };

  const renderTags = (tags: string) => {
    try {
      const parsedTags: string[] = JSON.parse(tags);
      return parsedTags
        .filter((t: string) => t !== null && t !== undefined)
        .map((t: string) => (
          <span key={t} className="badge mr-1">
            {t}
          </span>
        ));
    } catch {
      return null;
    }
  };

  const selectedProjectIds: number[] = selectedProjects.map((id) => Number(id));

  if (!hasMounted) return null;

  return (
    <div>
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
        <div>
          <h1 className="text-2xl font-bold text-info-content">Data Catalog</h1>
          <ProjectDropdown
            projects={projects}
            onSelectionChange={(ids) => setSelectedProjects(ids)}
            defaultSelected={fromProject ? [fromProject] : undefined}
          />
        </div>
      </div>
      <div className="flex justify-between gap-4 mb-4 pt-20 pl-8 w-full box-border">
        {/* Left side: Search bar + "Additional Filters" link */}
        <div className="flex flex-col md:w-1/2">
          <LargeSearchBar
            placeholder="Search"
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
            className="w-full"
          />
          <div className="text-right mt-1">
            <a
              href="/placeholder for advanced"
              className="text-sm underline text-secondary hover:underline"
            >
              Additional Filters
            </a>
          </div>
        </div>
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
          // TODO: populate list view appropriately
          <ListView
            data={tableData}
            activeSearchTerms={activeSearchTerms}
            selectedProjects={selectedProjectIds}
          />
        ) : (
          <GridView
            columns={[
              { header: "ID", data: "id" },
              {
                header: "Record Name",
                cell: (row) => (
                  <>
                    <Link
                      href={`/data_catalog/record?recordId=${row.id}&projectId=${row.projectId}`}
                      className="text-base-content font-bold hover:underline"
                    >
                      {row.name}
                    </Link>
                  </>
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
                cell: (row) => {
                  return renderTags(row.tags);
                  // return activeFilters ? (
                  //   <div>{renderTags(row.tags)}</div>
                  // ) : (
                  //   <ExpandableTagsCell tags={row.tags} />
                  // );
                },
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
