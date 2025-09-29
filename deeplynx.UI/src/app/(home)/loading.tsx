"use client"; // optional: add if react-loading-skeleton needs it
import {
  AdjustmentsHorizontalIcon,
  ArrowsRightLeftIcon,
  ArrowTrendingUpIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  CircleStackIcon,
  Cog6ToothIcon,
  DocumentDuplicateIcon,
  FolderIcon,
  MagnifyingGlassIcon,
  PlusCircleIcon,
  PlusIcon,
  RectangleGroupIcon,
  Squares2X2Icon,
} from "@heroicons/react/24/outline";
import React from "react";
import Skeleton from "react-loading-skeleton";
import WithT from "./components/WithT";
// NOTE: CSS import moved to layout or globals

export default function Loadingtranslations() {
  const totalPages = 2;
  const expandedIndex = 1;
  const columns = [1];
  const paginatedRecords = [1, 2, 3, 4, 5];
  return (
    <WithT>
      {(t) => (
        <div className="bg-base-100">
          {/* Header */}
          <div className="flex justify-between items-center bg-base-200/40 pl-12 pt-3 pb-2">
            <h1 className="text-2xl font-bold text-base-content">
              <Skeleton />
            </h1>
            <label
              className={`input input-bordered flex items-center relative mr-4`}
            >
              {/* Input field */}
              <input type="text" className="grow" />
              {/* Search icon */}
              <MagnifyingGlassIcon className="absolute left-3 top-2.5 w-5 h-5 text-base-content/50 size-6" />
            </label>
          </div>

          {/* Main Content */}
          <div className="mr-6 py-6">
            <div className="flex justify-between items-center justify-end mb-4">
              <button className="btn btn-outline btn-secondary flex items-center mr-2">
                <Cog6ToothIcon className="size-6" />
                {t.translations.CUSTOMIZE}
              </button>
              <button className="btn btn-secondary text-secondary-content flex items-center">
                <PlusIcon className="size-6" />
                {t.translations.WIDGET}
              </button>
            </div>
            <div className="flex">
              <div className="w-full md:w-1/2 px-4">
                <div className="card bg-base-200/20 border border-base-300 p-4">
                  <div className="flex justify-between items-center mb-4">
                    <h3 className="text-base-content text-lg font-semibold">
                      {t.translations.YOUR_PROJECTS}
                    </h3>

                    <div className="flex gap-2">
                      <button className="btn btn-outline btn-secondary flex items-center gap-1">
                        <PlusIcon className="size-6" />
                        <span>{t.translations.RECORD}</span>
                      </button>
                      <button className="btn btn-secondary text-secondary-content flex items-center gap-1">
                        <PlusIcon className="size-6" />
                        <span>{t.translations.PROJECT}</span>
                      </button>
                    </div>
                  </div>

                  <div>
                    <table className="table w-full border-separate p-2 border-spacing-y-2">
                      {expandedIndex === null && (
                        <thead>
                          <tr>
                            {columns.map((col, i) => (
                              <th key={i} className="text-base-content/70">
                                <Skeleton />
                              </th>
                            ))}
                            <th></th>
                          </tr>
                        </thead>
                      )}

                      <tbody>
                        {paginatedRecords.map((row, index) => {
                          const globalIndex = index; // because paginatedRecords index is local
                          return (
                            <React.Fragment key={globalIndex}>
                              <tr className="bg-base-200/30 hover:bg-base-200/50 rounded-lg overflow-hidden">
                                {columns.map((col, i) => (
                                  <td key={i} className="text-base-content">
                                    <Skeleton
                                      width={200}
                                      baseColor="var(--color-base-200)"
                                      highlightColor="var(--color-base-300)"
                                    />
                                    <Skeleton
                                      baseColor="var(--color-base-200)"
                                      highlightColor="var(--color-base-300)"
                                    />
                                    <Skeleton
                                      width={500}
                                      baseColor="var(--color-base-200)"
                                      highlightColor="var(--color-base-300)"
                                    />
                                  </td>
                                ))}
                              </tr>
                            </React.Fragment>
                          );
                        })}
                      </tbody>
                    </table>

                    {/* Pagination Controls */}
                    {totalPages > 1 && (
                      <div className="flex justify-end gap-2 mt-4">
                        <button className="btn btn-sm btn-ghost text-base-content">
                          <ChevronLeftIcon className="size-6" />
                        </button>
                        <span className="px-2 text-sm text-base-content">
                          <Skeleton
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </span>
                        <button className="btn btn-sm btn-ghost text-base-content">
                          <ChevronRightIcon className="size-6" />
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              </div>
              <div className="w-full md:w-1/2">
                <div className="card bg-base-200/20 border border-base-300">
                  <div className="card-body">
                    <div className="flex justify-between items-center">
                      <h2 className="card-title text-base-content">
                        {t.translations.LINKS}
                      </h2>
                      <button>
                        <PlusCircleIcon className="w-10 h-10 text-secondary hover:text-secondary-focus transition-colors" />
                      </button>
                    </div>
                    <div className="flex justify-between p-4">
                      <div className="flex flex-col items-center">
                        <AdjustmentsHorizontalIcon className="size-8 text-secondary" />
                        <button className="btn btn-link text-secondary hover:text-secondary-focus no-underline">
                          {t.translations.ROLES}
                        </button>
                      </div>
                      <div className="flex flex-col items-center">
                        <FolderIcon className="size-8 text-secondary" />
                        <button className="btn btn-link text-secondary hover:text-secondary-focus flex flex-col items-center no-underline">
                          {t.translations.FILE_EXPLORER}
                        </button>
                      </div>
                      <div className="flex flex-col items-center">
                        <DocumentDuplicateIcon className="size-8 text-secondary" />
                        <button className="btn btn-link text-secondary hover:text-secondary-focus flex flex-col items-center no-underline">
                          {t.translations.REPORTS}
                        </button>
                      </div>
                      <div className="flex flex-col items-center">
                        <ArrowTrendingUpIcon className="size-8 text-secondary" />
                        <button className="btn btn-link text-secondary hover:text-secondary-focus no-underline">
                          {t.translations.TRENDS}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="card bg-base-200/20 border border-base-300 mt-4">
                  <div className="card-body">
                    <h2 className="card-title text-base-content">
                      {t.translations.DATA_OVERVIEW}
                    </h2>
                    <div className="stats bg-base-100 shadow border border-base-300">
                      <div className="stat">
                        <div className="stat-title text-base-content/70">
                          {t.translations.PROJECTS}
                        </div>
                        <div className="stat-value text-primary flex items-center">
                          <Squares2X2Icon className="size-8 mr-2" />
                          <Skeleton
                            width={40}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </div>
                      </div>
                      <div className="stat">
                        <div className="stat-title text-base-content/70">
                          {t.translations.DATA_RECORD}
                        </div>
                        <div className="stat-value text-primary flex items-center">
                          <CircleStackIcon className="size-8 mr-2" />
                          <Skeleton
                            width={40}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </div>
                      </div>
                    </div>
                    <div className="stats bg-base-100 shadow border border-base-300">
                      <div className="stat">
                        <div className="stat-title text-base-content/70">
                          {t.translations.CLASSES}
                        </div>
                        <div className="stat-value text-primary flex items-center">
                          <RectangleGroupIcon className="size-8 mr-2" />
                          <Skeleton
                            width={40}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </div>
                      </div>
                      <div className="stat">
                        <div className="stat-title text-base-content/70">
                          {t.translations.CONNECTIONS}
                        </div>
                        <div className="stat-value text-primary flex items-center">
                          <ArrowsRightLeftIcon className="size-8 mr-2" />
                          <Skeleton
                            width={40}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </WithT>
  );
}
