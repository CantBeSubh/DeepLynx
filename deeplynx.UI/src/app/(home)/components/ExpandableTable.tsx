import { translations } from "@/app/lib/translations";
import {
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from "@heroicons/react/24/outline";
import React, { useState, ReactNode, useEffect } from "react";

interface ExpandableTableProps<T> {
  data: T[];
  columns: {
    header: string;
    data: (row: T) => ReactNode;
  }[];
  renderExpandedContent: (row: T, onClose: () => void) => ReactNode;
  onExplore: (row: T) => void;
}

const RECORDS_PER_PAGE = 5;

export function ExpandableTable<T>({
  data,
  columns,
  renderExpandedContent,
  onExplore,
}: ExpandableTableProps<T>) {
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];

  const toggleRow = (index: number) => {
    setExpandedIndex(expandedIndex === index ? null : index);
  };

  const closeExpanded = () => setExpandedIndex(null);

  const totalPages = Math.ceil(data.length / RECORDS_PER_PAGE);
  const startIndex = (currentPage - 1) * RECORDS_PER_PAGE;
  const paginatedRecords = data.slice(
    startIndex,
    startIndex + RECORDS_PER_PAGE
  );

  useEffect(() => {
    setExpandedIndex(null);
  }, [currentPage]);

  return (
    <div>
      <table className="table w-full border-separate p-2 border-spacing-y-2 shadow">
        {expandedIndex === null && (
          <thead>
            <tr>
              {columns.map((col, i) => (
                <th key={i}>{col.header}</th>
              ))}
              <th></th>
            </tr>
          </thead>
        )}

        <tbody>
          {paginatedRecords.map((row, index) => {
            const globalIndex = startIndex + index; // because paginatedRecords index is local
            return (
              <React.Fragment key={globalIndex}>
                {expandedIndex === globalIndex ? (
                  <tr>
                    <td colSpan={columns.length + 2} className="p-0">
                      <div className="overflow-hidden transition-all duration-500 ease-in-out max-h-[1000px] opacity-100">
                        <div className="card bg-base-200/40 p-6 rounded-box shadow">
                          {renderExpandedContent(row, closeExpanded)}
                        </div>
                      </div>
                    </td>
                  </tr>
                ) : (
                  <tr className="bg-base-200/20 hover:bg-base-200/40 rounded-lg overflow-hidden">
                    {columns.map((col, i) => (
                      <td key={i}>{col.data(row)}</td>
                    ))}
                    <td>
                      <button
                        className="btn btn-sm btn-outline btn-secondary hover:text-primary-content mr-3"
                        onClick={() => onExplore(row)}
                      >
                        {t.ExpandableTable.EXPLORE}
                      </button>
                    </td>
                    <td>
                      <button
                        onClick={() => toggleRow(globalIndex)}
                        aria-label="Expand row"
                      >
                        <ChevronDownIcon className="size-6" />
                      </button>
                    </td>
                  </tr>
                )}
              </React.Fragment>
            );
          })}
        </tbody>
      </table>

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
            {t.ExpandableTable.PAGE} {currentPage} {t.ExpandableTable.OF}{" "}
            {totalPages}
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
}
