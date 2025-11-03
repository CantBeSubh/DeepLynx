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
import WithT from "../components/WithT";
// NOTE: CSS import moved to layout or globals

export default function Loadingtranslations() {
  const totalPages = 2;
  const expandedIndex = 1;
  const columns = [1];
  const paginatedRecords = [1, 2];
  return (
    <WithT>
      {(t) => (
        <div>
          <div className="flex items-center bg-base-200/40 py-2 pl-12">
            <h1 className="text-2xl font-bold text-base-content">
              {t.translations.UPLOAD_CENTER}
            </h1>
          </div>

          {/* <div className="flex lg:flex-grow justify-between gap-8 p-10 lg:p-20"> */}
          <div
            className={`flex gap-8 p-10 lg:p-20 justify-center`}
          >
            {/* LEFT */}
            <div
              className={`w-full lg:w-3/5`}
            >
              <h2>{t.translations.START_UPLOAD_BY_CHOOSING_TYPE}</h2>
              <div className="p-4 space-y-4">
                <table className="table w-full border-separate p-2 border-spacing-y-4">
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
                              <React.Fragment key={i}>
                                <td key={i} className="text-base-content">
                                  <Skeleton
                                    width={100}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td><td key={i + 1} className="text-base-content">
                                  <Skeleton
                                    width={100}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td><td key={i + 2} className="text-base-content">
                                  <Skeleton
                                    width={100}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td>
                              </React.Fragment>
                            )
                            )}
                          </tr>
                        </React.Fragment>
                      );
                    })}
                  </tbody>
                </table>
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
                              <React.Fragment key={i}>
                                <td key={i} className="text-base-content">
                                  <Skeleton
                                    width={100}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td><td key={i + 1} className="text-base-content">
                                  <Skeleton
                                    width={350}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td><td key={i + 2} className="text-base-content">
                                  <Skeleton
                                    width={75}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td><td key={i + 3} className="text-base-content">
                                  <Skeleton
                                    width={50}
                                    baseColor="var(--color-base-200)"
                                    highlightColor="var(--color-base-300)" />
                                </td>
                              </React.Fragment>
                            )
                            )}
                          </tr>
                        </React.Fragment>
                      );
                    })}
                  </tbody>
                </table>

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
                      const globalIndex = index;
                      return (
                        <tr key={globalIndex} className="bg-base-200/30 hover:bg-base-200/50 rounded-lg overflow-hidden">
                          <td className="text-base-content">
                            <Skeleton
                              width={100}
                              baseColor="var(--color-base-200)"
                              highlightColor="var(--color-base-300)"
                            />
                          </td>
                          <td className="text-base-content">
                            <Skeleton
                              width={350}
                              baseColor="var(--color-base-200)"
                              highlightColor="var(--color-base-300)"
                            />
                          </td>
                          <td className="text-base-content">
                            <Skeleton
                              width={75}
                              baseColor="var(--color-base-200)"
                              highlightColor="var(--color-base-300)"
                            />
                          </td>
                          <td className="text-base-content">
                            <Skeleton
                              width={50}
                              baseColor="var(--color-base-200)"
                              highlightColor="var(--color-base-300)"
                            />
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>

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
                    {paginatedRecords.map((row, index) => (
                      <tr key={index} className="bg-base-200/30 hover:bg-base-200/50 rounded-lg overflow-hidden">
                        <td className="text-base-content">
                          <Skeleton
                            width={100}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </td>
                        <td className="text-base-content">
                          <Skeleton
                            width={350}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </td>
                        <td className="text-base-content">
                          <Skeleton
                            width={75}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </td>
                        <td className="text-base-content">
                          <Skeleton
                            width={50}
                            baseColor="var(--color-base-200)"
                            highlightColor="var(--color-base-300)"
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>

      )}
    </WithT>
  );
}
