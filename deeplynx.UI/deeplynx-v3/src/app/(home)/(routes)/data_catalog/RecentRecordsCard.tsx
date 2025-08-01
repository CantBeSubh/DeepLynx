import { getRecentlyAddedRecords } from "@/app/lib/user_services";
import { useEffect, useState } from "react";

export type RecentRecord = {
  id: number;
  name: string;
  className: string;
  lastUpdatedAt: string;
  dataSourceName: string;
  projectName: string;
};

const RecentRecordsCard = ({
  selectedProjects,
}: {
  selectedProjects: string[];
}) => {
  const [records, setRecords] = useState<RecentRecord[]>([]);

  useEffect(() => {
    const fetchRecentRecords = async () => {
      try {
        const data = await getRecentlyAddedRecords(selectedProjects);
        setRecords(data);
      } catch (error) {
        console.error("Failed to fetch recent records:", error);
      }
    };

    if (selectedProjects && selectedProjects.length > 0) {
      fetchRecentRecords();
    }
  }, [selectedProjects]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const options: Intl.DateTimeFormatOptions = {
      month: 'long', 
      day: 'numeric', 
      year: 'numeric', 
      hour: 'numeric', 
      minute: 'numeric', 
      hour12: true, 
      timeZoneName: 'short'
    };
    return date.toLocaleString('en-US', options);
  };

  return (
    <div className="bg-base-100 rounded-xl p-4">
      <h2 className="text-lg font-semibold mb-4 border-b border-base-content">
        Recent Added Records
      </h2>
      <ul className="list px-4">
        {records.map((record, index) => (
          <li key={index} className="py-4 border-b border-base-content">
            <div className="font-bold text-base-content mb-1">
              {record.name}
            </div>
            <div className="text-sm text-base-300 space-x-2 flex flex-wrap">
              <span>
                Class:{" "}
                <span className="badge badge-info badge-sm text-xs">
                  {record.className}
                </span>
              </span>
              <span className="ml-4">Last Edited: {formatDate(record.lastUpdatedAt)}</span>
              <span className="ml-4">Project: {record.projectName}</span>
              <span className="ml-4">Data Source: {record.dataSourceName}</span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default RecentRecordsCard;
