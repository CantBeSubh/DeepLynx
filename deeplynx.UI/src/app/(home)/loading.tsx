// "use client"; // optional: add if react-loading-skeleton needs it
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
import { translations } from "../lib/translations";
// NOTE: CSS import moved to layout or globals

export default function Loadingtranslations() {
  const locale = "en";
  const t = translations[locale];
  const totalPages = 2;
  const expandedIndex = 1;
  const columns = [1];
  const paginatedRecords = [1, 2, 3, 4, 5];
  return (
    <div className="bg-base-100">
      {/* Header */}
      <div className="flex justify-between items-center bg-base-200/40 pl-12 pt-3 pb-2">
        <h1 className="text-2xl font-bold text-info-content">
          <Skeleton />
        </h1>
        <label className={`input flex items-center relative mr-4`}>
          {/* Input field */}
          <input type="text" />
          {/* Search icon */}
          <MagnifyingGlassIcon className="absolute left-3 top-2.5 w-5 h-5 text-base-content size-6" />
        </label>
      </div>

      {/* Main Content */}
      <div className="mr-6 py-6">
        <div className="flex justify-between items-center justify-end mb-4">
          <button className="btn btn-outline btn-secondary flex items-center mr-2">
            <Cog6ToothIcon className="size-6" />
            {t.translations.CUSTOMIZE}
          </button>
          <button className="btn btn-secondary text-primary-content flex items-center">
            <PlusIcon className="size-6" />
            {t.translations.WIDGET}
          </button>
        </div>
        <div className="flex">
          <div className="w-full md:w-1/2 px-4">
            <div className="card card-border p-4">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-info-content text-lg font-semibold">
                  {t.translations.YOUR_PROJECTS}
                </h3>

                <div className="flex gap-2">
                  <button className="btn btn-outline btn-secondary flex items-center gap-1">
                    <PlusIcon className="size-6" />
                    <span>{t.translations.RECORD}</span>
                  </button>
                  <button className="btn btn-secondary text-primary-content flex items-center gap-1">
                    <PlusIcon className="size-6" />
                    <span>{t.translations.PROJECT}</span>
                  </button>
                </div>
              </div>

              <div>
                <table className="table w-full border-separate p-2 border-spacing-y-2 shadow">
                  {expandedIndex === null && (
                    <thead>
                      <tr>
                        {columns.map((col, i) => (
                          <th key={i}>
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
                          <tr className="bg-base-200/20 hover:bg-base-200/40 rounded-lg overflow-hidden">
                            {columns.map((col, i) => (
                              <td key={i}>
                                <Skeleton width={200} />
                                <Skeleton />
                                <Skeleton width={500} />
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
                    <button className="btn btn-sm btn-ghost">
                      <ChevronLeftIcon className="size-6" />
                    </button>
                    <span className="px-2 text-sm">
                      <Skeleton />
                    </span>
                    <button className="btn btn-sm btn-ghost">
                      <ChevronRightIcon className="size-6" />
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
          <div className="w-full md:w-1/2">
            <div className="card card-border">
              <div className="card-body">
                <div className="flex justify-between items-center">
                  <h2 className="card-title">{t.translations.LINKS}</h2>
                  <button>
                    <PlusCircleIcon className="w-10 h-10 text-secondary" />
                  </button>
                </div>
                <div className="flex justify-between p-4">
                  <div className="flex flex-col items-center">
                    <AdjustmentsHorizontalIcon className="size-8 text-secondary" />
                    <button className="btn btn-link text-secondary">
                      {t.translations.ROLES}
                    </button>
                  </div>
                  <div className="flex flex-col items-center">
                    <FolderIcon className="size-8 text-secondary" />
                    <button className="btn btn-link text-secondary flex flex-col items-center">
                      {t.translations.FILE_EXPLORER}
                    </button>
                  </div>
                  <div className="flex flex-col items-center">
                    <DocumentDuplicateIcon className="size-8 text-secondary" />
                    <button className="btn btn-link text-secondary flex flex-col items-center">
                      {t.translations.REPORTS}
                    </button>
                  </div>
                  <div className="flex flex-col items-center">
                    <ArrowTrendingUpIcon className="size-8 text-secondary" />
                    <button className="btn btn-link text-secondary ">
                      {t.translations.TRENDS}
                    </button>
                  </div>
                </div>
              </div>
            </div>
            <div className="card card-border mt-4">
              <div className="card-body">
                <h2 className="card-title">{t.translations.DATA_OVERVIEW}</h2>
                <div className="stats shadow">
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.translations.PROJECTS}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <Squares2X2Icon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.translations.DATA_RECORD}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <CircleStackIcon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                </div>
                <div className="stats shadow">
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.translations.CLASSES}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <RectangleGroupIcon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.translations.CONNECTIONS}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <ArrowsRightLeftIcon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
