import { getRecentlyAddedRecords } from "@/app/lib/user_services";
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
    <div className="bg-base-100 rounded-xl p-4 shadow-md">
      <h2 className="text-lg text-black mb-4t">Recently Added Records</h2>
      <div className="divider m-0 mt-2"></div>
      <ul className="list mt-0">
        {paginatedRecords.map((record, index) => (
          <li
            key={index}
            className="border-b border-base-content cursor-pointer hover:bg-base-200/30 p-2 pl-0 rounded-sm"
            onClick={() =>
              router.push(
                `/data_catalog/record?recordId=${record.id}&projectId=${record.projectId}`
              )
            }
          >
            <div className="text-accent-content mb-1">{record.name}</div>
            <div className="text-sm text-base-300 space-x-2 flex flex-wrap">
              <span>
                Class:{" "}
                <span className="badge badge-info badge-sm text-xs">
                  {record.className}
                </span>
              </span>
              <span className="ml-4">
                Last Edited:{" "}
                {formatDate(record.lastUpdatedAt ?? record.createdAt)}
              </span>
              <span className="ml-4">Project: {record.projectName}</span>
              <span className="ml-4">Data Source: {record.dataSourceName}</span>
            </div>
          </li>
        ))}
      </ul>

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex justify-end gap-2 mt-4">
          <button
            className="btn btn-sm btn-ghost"
            disabled={currentPage === 1}
            onClick={() => setCurrentPage((prev) => prev - 1)}
          >
            <ChevronLeftIcon className="size-6" />
          </button>
          <span className="px-2 text-sm">
            Page {currentPage} of {totalPages}
          </span>
          <button
            className="btn btn-sm btn-ghost"
            disabled={currentPage === totalPages}
            onClick={() => setCurrentPage((prev) => prev + 1)}
          >
            <ChevronRightIcon className="size-6" />
          </button>
        </div>
      )}
    </div>
  );
};

export default RecentRecordsCard;
