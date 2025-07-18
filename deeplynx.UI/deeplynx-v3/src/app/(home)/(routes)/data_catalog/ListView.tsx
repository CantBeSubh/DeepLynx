import React from "react";
import { FileViewerTableRow } from "../../types/types";
import Link from "next/link";

export type RecordsListView = {
  id: number;
  uri: string;
  properties: string;
  originalId: string;
  name: string;
  classId: number;
  dataSourceId: number;
  projectId: number;
  createdBy: string;
  createdAt: string;
  modifiedBy: string | null;
  modifiedAt: string | null;
  archivedAt: string | null;
};

interface ListViewProps {
  records: FileViewerTableRow[];
  activeSearchTerms?: string[];
}

const ListView: React.FC<ListViewProps> = ({
  records,
  activeSearchTerms = [],
}) => {
  const getHighlightedCell = (text: unknown, queries: string[]) => {
    const safeText = String(text);
    if (!queries.length) return { content: safeText, matched: false };

    const lowerText = safeText.toLowerCase();
    const match = queries.find((q) => lowerText.includes(q.toLowerCase()));

    if (!match) return { content: safeText, matched: false };

    const regex = new RegExp(`(${match})`, "gi");
    const parts = safeText.split(regex);

    const content = parts.map((part, index) =>
      regex.test(part) ? (
        <span
          key={index}
          className="font-bold text-info-content bg-info rounded px-1"
        >
          {part}
        </span>
      ) : (
        part
      )
    );
    return { content, matched: true };
  };

  return (
    <div className="bg-base-100 rounded-xl shadow p-4 w-full mx-auto">
      <ul className="list">
        {records.map((record, index) => {
          const name = getHighlightedCell(record.name, activeSearchTerms);
          // const desc = getHighlightedCell(
          //   record.fileDescription,
          //   activeSearchTerms
          // );
          const date = getHighlightedCell(record.modifiedAt, activeSearchTerms);

          return (
            <li key={index} className="py-4 border-b border-base-content">
              <div className={`font-bold text-base-content mb-1  `}>
                {name.content}
              </div>
              {/* <span className="text-sm">{desc.content}</span> */}
              <div className="flex pt-2">
                {record.className && (
                  <span className="font-bold">
                    Class:
                    <span className="badge badge-sm text-xs ml-2">
                      Timeseries
                    </span>
                  </span>
                )}
                <div className="ml-4">
                  <span className="font-bold">Last Edited: </span>{" "}
                  {date.content}
                </div>
                {/* <div className="ml-4">
                  <span className="font-bold">File Type: </span>{" "}
                  {record.fileType}
                </div> */}
              </div>
              <div className="pt-2">
                <span>Tags: </span>
                {record.tags.map((tag, index) => (
                  <span key={index} className={`badge ml-2`}>
                    {tag}
                  </span>
                ))}
              </div>
              {/* <div className="pt-2">
                <span className="font-bold">Associated Records: </span>
                {record.associatedRecords?.map((record, index) => (
                  <Link
                    key={index}
                    href={"#"}
                    className="border-b text-blue-600 mr-3"
                  >
                    {record}
                  </Link>
                ))}
              </div> */}
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default ListView;
