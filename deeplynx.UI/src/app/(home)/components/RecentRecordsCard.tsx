"use client";

import { useLanguage } from "@/app/contexts/Language";
import { getRecentlyAddedRecords } from "@/app/lib/user_services.client";
import { ChevronLeftIcon, ChevronRightIcon } from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

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

const RECORDS_PER_PAGE = 5;

const RecentRecordsCard = ({
  selectedProjects,
}: {
  selectedProjects: string[];
}) => {
  const { t } = useLanguage();
  const router = useRouter();
  const [records, setRecords] = useState<RecentRecord[]>([]);
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    const fetchRecentRecords = async () => {
      try {
        const data = await getRecentlyAddedRecords(selectedProjects);
        setRecords(data);
        setCurrentPage(1);
      } catch (error) {
        console.error("Failed to fetch recent records:", error);
      }
    };

    if (selectedProjects && selectedProjects.length > 0) {
      fetchRecentRecords();
    }
  }, [selectedProjects]);

  const totalPages = Math.ceil(records.length / RECORDS_PER_PAGE);
  const startIndex = (currentPage - 1) * RECORDS_PER_PAGE;
  const paginatedRecords = records.slice(
    startIndex,
    startIndex + RECORDS_PER_PAGE
  );

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

  return (
    <div className="shadow shadow-md rounded-xl">
      {/* Header */}
      <h2 className="text-lg font-semibold text-base-content mb-2 p-4">
        {t.translations.RECENTLY_ADDED_RECORDS}
      </h2>
      <div className="divider m-0"></div>

      {/* Records List */}
      <ul className="space-y-1 p-2">
        {paginatedRecords.map((record, index) => (
          <li
            key={index}
            className="border-b border-base-300/30 cursor-pointer hover:bg-base-200/40 p-3 -mx-1 transition-colors"
            onClick={() =>
              router.push(
                `/record?recordId=${record.id}&projectId=${record.projectId}`
              )
            }
          >
            {/* Record Name */}
            <div className="font-medium text-base-content mb-2">
              {record.name}
            </div>

            {/* Record Metadata */}
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

      {/* Empty State */}
      {paginatedRecords.length === 0 && (
        <div className="text-center py-8 text-base-content/60">
          {t.translations.NO_RECENT_RECORDS || "No recent records found"}
        </div>
      )}

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex justify-end items-center gap-2 p-4 border-base-300/30">
          <button
            className="btn btn-sm btn-ghost hover:bg-base-200"
            disabled={currentPage === 1}
            onClick={() => setCurrentPage((prev) => prev - 1)}
          >
            <ChevronLeftIcon className="w-5 h-5 text-base-content/70" />
          </button>
          <span className="px-3 text-sm text-base-content/80 font-medium">
            {t.translations.PAGE} {currentPage} {t.translations.OF} {totalPages}
          </span>
          <button
            className="btn btn-sm btn-ghost hover:bg-base-200"
            disabled={currentPage === totalPages}
            onClick={() => setCurrentPage((prev) => prev + 1)}
          >
            <ChevronRightIcon className="w-5 h-5 text-base-content/70" />
          </button>
        </div>
      )}
    </div>
  );
};

export default RecentRecordsCard;
