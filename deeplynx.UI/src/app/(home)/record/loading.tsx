// app/(home)/(routes)/data_catalog/record/loading.tsx
"use client";

import Skeleton from "react-loading-skeleton";
import { useSearchParams } from "next/navigation";

// Small helpers so maps always have keys
const times = (n: number) => Array.from({ length: n }, (_, i) => i);

export default function RecordLoading() {
  // Optional: grab recordId/projectId to tweak the headline while loading
  const params = useSearchParams();
  const rid = params.get("recordId");

  return (
    <div className="px-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-info-content">
          {rid ? <Skeleton width={280} /> : <Skeleton width={220} />}
        </h1>
      </div>

      <div className="divider" />

      {/* Tabs header skeleton */}
      <div className="tabs tabs-bordered mb-4">
        <a className="tab tab-active">
          <Skeleton width={160} height={20} />
        </a>
        <a className="tab">
          <Skeleton width={140} height={20} />
        </a>
        <a className="tab">
          <Skeleton width={120} height={20} />
        </a>
        <a className="tab">
          <Skeleton width={140} height={20} />
        </a>
      </div>

      {/* Active tab content skeleton = “Record Information” */}
      <div className="flex w-full gap-4">
        {/* Left column: Property tables */}
        <div className="w-full md:w-1/2 p-3 px-4">
          <PropertyTableSkeleton titleWidth={180} rows={8} />
          <PropertyTableSkeleton className="mt-4" titleWidth={200} rows={6} />
        </div>

        {/* Right column: Tags card */}
        <div className="flex-grow">
          <div className="card bg-base-100 shadow-md p-4">
            <div className="card-title mb-2">
              <Skeleton width={120} />
            </div>
            <div className="flex gap-2 flex-wrap">
              {times(6).map((i) => (
                <Skeleton key={i} width={60} height={24} />
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

/** Light skeleton for a property table (don’t import real PropertyTable here) */
function PropertyTableSkeleton({
  titleWidth = 160,
  rows = 6,
  className = "",
}: {
  titleWidth?: number;
  rows?: number;
  className?: string;
}) {
  return (
    <div className={`card bg-base-100 shadow-md p-3 ${className}`}>
      <div className="mb-3">
        <Skeleton width={titleWidth} height={20} />
      </div>
      <div className="divide-y divide-base-200">
        {times(rows).map((i) => (
          <div key={i} className="py-3 grid grid-cols-3 gap-4 items-center">
            <Skeleton height={18} />
            <div className="col-span-2">
              <Skeleton height={18} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
