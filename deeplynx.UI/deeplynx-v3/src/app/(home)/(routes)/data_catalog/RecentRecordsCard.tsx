import React from "react";
import SavedSearchesTabs from "../../components/SavedSearches";

export type RecentRecord = {
  id: number;
  name: string;
  className: string;
  lastEdited: string;
  fileType: string;
  projectName: string;
};

const RecentRecordsCard = ({ records }: { records: RecentRecord[] }) => {
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
              <span className="ml-4">Last edited: {record.lastEdited}</span>
              <span className="ml-4">File type: {record.fileType}</span>
              <span className="ml-4">Project: {record.projectName}</span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default RecentRecordsCard;
