import React from "react";
import { Column } from "@/app/(home)/types/types";

type GridViewProps<T extends object> = {
  columns: Column<T>[];
  data: T[];
  activeSearchTerms?: string[];
};

const GridView = <T extends object>({
  columns,
  data,
  activeSearchTerms = [],
}: GridViewProps<T>) => {
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
    <div className="h-150 overflow-x-auto">
      <table className="table table-pin-rows table-pin-cols">
        {/* Headers */}
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
          {/* Table Rows */}
          {data.map((row, rowIndex) => {
            return (
              <tr key={rowIndex}>
                {columns.map((column, colIndex) => {
                  return (
                    <td
                      key={colIndex}
                      className="text-base-content border border-base-200"
                    >
                      {column.cell
                        ? column.cell(row)
                        : (row[column.data as keyof T] as React.ReactNode)}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};

export default GridView;
