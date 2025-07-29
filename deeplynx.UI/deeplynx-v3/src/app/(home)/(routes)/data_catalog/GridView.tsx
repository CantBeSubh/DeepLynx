import React from "react";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";

type GridViewProps = {
  columns: Column<FileViewerTableRow>[];
  data: FileViewerTableRow[];
  activeSearchTerms?: string[];
  selectedProjects?: string[];
};

const GridView = <T extends object>({
  columns,
  data,
  activeSearchTerms = [],
  selectedProjects,
}: GridViewProps) => {
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

  const filteredRecords =
    selectedProjects?.includes("All your Projects") || !selectedProjects
      ? data
      : data.filter((record) =>
          selectedProjects.includes(record.projectName ?? "")
        );

  return (
    <div className="h-150 overflow-x-auto">
      <table className="table table-pin-rows table-pin-cols">
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th
                key={index}
                className="text-base-content border border-base-200 bg-info/30"
              >
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {filteredRecords.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {columns.map((column, colIndex) => {
                const rawValue = column.data ? row[column.data] : "";

                return (
                  <td
                    key={colIndex}
                    className="text-base-content border border-base-200"
                  >
                    {column.cell
                      ? column.cell(row)
                      : getHighlightedCell(rawValue, activeSearchTerms).content}
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default GridView;
