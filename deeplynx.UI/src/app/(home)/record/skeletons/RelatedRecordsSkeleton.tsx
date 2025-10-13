// src/app/(home)/record/skeletons/RelatedRecordsSkeleton.tsx

"use client";

import React from "react";

interface RelatedRecordsCardSkeletonProps {
  title?: string;
  /** Number of skeleton rows to show */
  rows?: number;
  /** Number of columns to show */
  columns?: number;
}

function RelatedRecordsCardSkeleton({
  title = "Related Records:",
  rows = 3,
  columns = 4,
}: RelatedRecordsCardSkeletonProps) {
  return (
    <div className="card bg-base-100 shadow-md mt-4 p-2 animate-pulse">
      <h2 className="text-xl font-bold md-4 text-base-content">{title}</h2>
      <div className="card-body p-4">
        <div className="overflow-x-auto rounded-box border border-base-300 bg-base-100">
          <table className="table">
            <thead>
              <tr className="bg-base-200 text-base-content">
                {Array.from({ length: columns }).map((_, i) => (
                  <th key={i}>
                    <div className="h-4 bg-base-300 rounded w-20 animate-pulse" />
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {Array.from({ length: rows }).map((_, rowIndex) => (
                <tr key={rowIndex}>
                  {Array.from({ length: columns }).map((_, colIndex) => (
                    <td key={colIndex}>
                      <div className="space-y-2">
                        <div
                          className="h-4 bg-base-300 rounded animate-pulse"
                          style={{
                            width:
                              colIndex === 0
                                ? "4rem"
                                : colIndex === columns - 1
                                ? "4rem"
                                : `${Math.floor(Math.random() * 40) + 60}%`,
                          }}
                        />
                      </div>
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default RelatedRecordsCardSkeleton;
