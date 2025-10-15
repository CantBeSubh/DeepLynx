// src/app/(home)/components/RecentRecordsCard.tsx
"use client";
import {
  BarsArrowDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from "@heroicons/react/24/outline";
import { useLanguage } from "@/app/contexts/Language";
import { getRecentlyAddedRecords } from "@/app/lib/user_services.client";
import { useRouter } from "next/navigation";
import React, { useEffect, useState, useCallback, useMemo } from "react";
import CatalogViewSkeleton from "./skeletons/catalogviewskeleton";

export type RecentRecord = {
  id: number;
  name: string;
  className: string;
  createdAt: string;
  lastUpdatedAt: string;
  dataSourceName: string;
  projectName: string;
  projectId: number;
};

interface Props {
  selectedProjects: string[];
  border?: boolean;
}

const RECORDS_PER_PAGE = 5;

const RecentRecordsCard: React.FC<Props> = ({
  selectedProjects,
  border = true,
}) => {
  const { t } = useLanguage();
  const router = useRouter();

  const [records, setRecords] = useState<RecentRecord[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // single select option (no separate direction)
  type SortOption = "nameAZ" | "nameZA" | "dateNew" | "dateOld";
  const [sortOption, setSortOption] = useState<SortOption>("nameAZ");

  const fetchRecentRecords = useCallback(async () => {
    if (!selectedProjects || selectedProjects.length === 0) {
      setRecords([]);
      setCurrentPage(1);
      return;
    }
    setIsLoading(true);
    setError(null);
    try {
      const data = await getRecentlyAddedRecords(selectedProjects);
      setRecords(Array.isArray(data) ? data : []);
      setCurrentPage(1);
    } catch (e) {
      console.error("Failed to fetch recent records:", e);
      setError("Failed to load recent records.");
      setRecords([]);
    } finally {
      setIsLoading(false);
    }
  }, [selectedProjects]);

  useEffect(() => {
    fetchRecentRecords();
  }, [fetchRecentRecords]);

  // reset to first page whenever sort changes
  useEffect(() => {
    setCurrentPage(1);
  }, [sortOption]);

  const sorted = useMemo(() => {
    const arr = [...records];
    arr.sort((a, b) => {
      const dateA = new Date(a.lastUpdatedAt || a.createdAt).getTime();
      const dateB = new Date(b.lastUpdatedAt || b.createdAt).getTime();

      switch (sortOption) {
        case "nameAZ":
          return a.name.localeCompare(b.name, undefined, {
            sensitivity: "base",
          });
        case "nameZA":
          return b.name.localeCompare(a.name, undefined, {
            sensitivity: "base",
          });
        case "dateNew":
          return dateB - dateA;
        case "dateOld":
          return dateA - dateB;
        default:
          return 0;
      }
    });
    return arr;
  }, [records, sortOption]);

  const totalPages = Math.max(1, Math.ceil(sorted.length / RECORDS_PER_PAGE));
  const startIndex = (currentPage - 1) * RECORDS_PER_PAGE;
  const paginatedRecords = sorted.slice(
    startIndex,
    startIndex + RECORDS_PER_PAGE
  );

  const handleSortChange = (val: SortOption) => setSortOption(val);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const options: Intl.DateTimeFormatOptions = {
      month: "long",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "numeric",
      hour12: true,
      timeZoneName: "short",
    };
    return date.toLocaleString("en-US", options);
  };

  if (isLoading) return <CatalogViewSkeleton />;

  return (
    <div className={border ? "shadow shadow-md rounded-xl" : ""}>
      {/* Header + Sort */}
      <div className="flex items-center justify-between p-4">
        <h2 className="text-lg font-semibold text-base-content">
          {t.translations.RECENTLY_ADDED_RECORDS}
        </h2>
        <div className="flex items=cemter gap-1">
          <div className="px-3 py-2 text-md font-semibold text-base-content/50">
            Sort By
          </div>
          <div className="relative inline-block">
            <BarsArrowDownIcon
              className={`pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 h-5 w-5 transition-colors
                          text-[var(--color-gray-400)]`}
            />
            <select
              value={sortOption}
              onChange={(e) => handleSortChange(e.target.value as SortOption)}
              className={`appearance-none border-2 border-gray-400 square-lg pl-3 pr-9 py-2 text-sm 
                          bg-white font-semibold text-base-content/50 cursor-pointer
                          hover:bg-[var(--color-secondary)] hover:text-[var(--color-base-200)] focus:ring-2 focus:ring-[var(--color-secondary)]
                          transition-all duration-200 w-44`}
            >
              <option value="nameAZ">Name: A to Z</option>
              <option value="nameZA">Name: Z to A</option>
              <option value="dateNew">Date: Newest</option>
              <option value="dateOld">Date: Oldest</option>
            </select>
          </div>
        </div>
      </div>

      <div className="divider m-0"></div>

      {/* Error */}
      {error && (
        <div className="p-4 text-error flex items-center justify-between">
          <span>{error}</span>
          <button
            className="btn btn-sm btn-outline"
            onClick={fetchRecentRecords}
          >
            Retry
          </button>
        </div>
      )}

      {/* List */}
      <ul className="space-y-1 p-2">
        {paginatedRecords.map((record) => (
          <li
            key={record.id}
            className="border-b border-base-300/30 cursor-pointer hover:bg-base-200/40 p-3 -mx-1 transition-colors"
            onClick={() =>
              router.push(
                `/record?recordId=${record.id}&projectId=${record.projectId}`
              )
            }
          >
            <div className="font-medium text-base-content mb-2">
              {record.name}
            </div>

            <div className="text-sm text-base-content/60 flex flex-wrap gap-x-4 gap-y-1">
              <span className="flex items-center gap-1">
                <span>{t.translations.CLASS}:</span>
                <span className="badge badge-sm badge-secondary">
                  {record.className}
                </span>
              </span>

              <span>
                <span className="text-base-content/50">
                  {t.translations.LAST_EDIT}:
                </span>{" "}
                {formatDate(record.lastUpdatedAt ?? record.createdAt)}
              </span>

              <span>
                <span className="text-base-content/50">
                  {t.translations.PROJECT}:
                </span>{" "}
                {record.projectName}
              </span>

              <span>
                <span className="text-base-content/50">
                  {t.translations.DATA_SOURCE}:
                </span>{" "}
                {record.dataSourceName}
              </span>
            </div>
          </li>
        ))}
      </ul>

      {/* Empty */}
      {!error && paginatedRecords.length === 0 && (
        <div className="text-center py-8 text-base-content/60">
          {t.translations.NO_RECENT_RECORDS || "No recent records found"}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-end items-center gap-2 p-4 border-base-300/30">
          <button
            className="btn btn-sm btn-ghost hover:bg-base-200"
            disabled={currentPage === 1}
            onClick={() => setCurrentPage((p) => p - 1)}
          >
            <ChevronLeftIcon className="w-5 h-5 text-base-content/70" />
          </button>
          <span className="px-3 text-sm text-base-content/80 font-medium">
            {t.translations.PAGE} {currentPage} {t.translations.OF} {totalPages}
          </span>
          <button
            className="btn btn-sm btn-ghost hover:bg-base-200"
            disabled={currentPage === totalPages}
            onClick={() => setCurrentPage((p) => p + 1)}
          >
            <ChevronRightIcon className="w-5 h-5 text-base-content/70" />
          </button>
        </div>
      )}
    </div>
  );
};

export default RecentRecordsCard;
