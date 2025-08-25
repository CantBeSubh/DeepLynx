"use client";

import { Column } from "@/app/(home)/types/types";
import {
  ChevronDownIcon,
  ChevronUpIcon,
  FolderIcon,
  LockClosedIcon,
  PresentationChartLineIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import React, { useState } from "react";
import SearchInput from "./SearchInput";
import { translations } from "@/app/lib/translations";

// Define the props for the GenericTable component
type GenericTableProps<T extends object> = {
  columns: Column<T>[];
  data: T[];
  filterPlaceholder?: string;
  isAnyRowSelected?: boolean;
  deleteSelectedRows?: () => void;
  rowsPerPage?: number;
  enablePagination?: boolean;
  bordered?: boolean;
  searchBar?: boolean;
  actionButtons?: boolean;
  rowClassName?: string | ((row: T, index: number) => string); // Allows certain rows get styled based on conditions ex: rowClassName={(row) => (row.status === "error" ? "bg-red-100" : "")}
  tableClassName?: string; // This is for table styling such as grid style
  gridView?: boolean;
};

const GenericTable = <T extends object>({
  columns,
  data,
  filterPlaceholder,
  isAnyRowSelected,
  deleteSelectedRows,
  rowsPerPage = 10, // Default value for rowsPerPage
  enablePagination = false, // Default value for enablePagination
  bordered = false, // Default value for bordered
  searchBar = false, // Default value for searchBar
  actionButtons = false, // Default value for actionButtons
  rowClassName,
  tableClassName,
  gridView = false,
}: GenericTableProps<T>) => {
  const locale = "en";
  const t = translations[locale];
  const [filterText, setFilterText] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Filter data based on the search input
  const filteredData = data.filter((row) =>
    columns.some((column) =>
      row[column.data as keyof T]
        ?.toString()
        .toLowerCase()
        .includes(filterText.toLowerCase())
    )
  );

  // State and logic for column sorting
  const [sortConfig, setSortConfig] = useState<{
    key: keyof T;
    direction: "asc" | "desc";
  } | null>(null);

  // Memoize sorted data to avoid unnecessary calculations
  const sortedData = React.useMemo(() => {
    if (!sortConfig) return filteredData;

    return [...filteredData].sort((a, b) => {
      const aValue = a[sortConfig.key as keyof T];
      const bValue = b[sortConfig.key as keyof T];

      if (aValue === null) return 1;
      if (bValue === null) return -1;

      if (typeof aValue === "string" && typeof bValue === "string") {
        return sortConfig.direction === "asc"
          ? aValue.localeCompare(bValue)
          : bValue.localeCompare(aValue);
      }

      if (typeof aValue === "number" && typeof bValue === "number") {
        return sortConfig.direction === "asc"
          ? aValue - bValue
          : bValue - aValue;
      }

      return 0;
    });
  }, [filteredData, sortConfig]);

  // Calculate total pages for pagination
  const totalPages = enablePagination
    ? Math.ceil(filteredData.length / rowsPerPage)
    : 1;

  // Get data for the current page
  const currentData = enablePagination
    ? sortedData.slice(
      (currentPage - 1) * rowsPerPage,
      currentPage * rowsPerPage
    )
    : sortedData;

  // Handle page click for pagination
  const handlePageClick = (pageNumber: number) => {
    setCurrentPage(pageNumber);
  };

  // Create pagination buttons
  const createPagination = () => {
    const pagination = [];

    if (totalPages <= 6) {
      for (let i = 1; i <= totalPages; i++) {
        pagination.push(
          <button
            key={i}
            className={`join-item btn ${currentPage === i ? "btn-primary" : ""
              }`}
            onClick={() => handlePageClick(i)}
          >
            {i}
          </button>
        );
      }
    } else {
      if (currentPage > 1) {
        pagination.push(
          <button
            key="prev"
            className="join-item btn"
            onClick={() => handlePageClick(currentPage - 1)}
          >
            {t.translations.PREV}
          </button>
        );
      }

      for (let i = 1; i <= Math.min(3, totalPages); i++) {
        pagination.push(
          <button
            key={i}
            className={`join-item btn ${currentPage === i ? "btn-primary" : ""
              }`}
            onClick={() => handlePageClick(i)}
          >
            {i}
          </button>
        );
      }

      if (currentPage > 3 && currentPage <= totalPages - 3) {
        pagination.push(
          <span key="ellipsis1" className=" btn join-item btn-disabled">
            ...
          </span>
        );
        pagination.push(
          <button
            key={currentPage}
            className="join-item btn btn-primary"
            onClick={() => handlePageClick(currentPage)}
          >
            {currentPage}
          </button>
        );
        pagination.push(
          <span key="ellipsis2" className="btn join-item btn-disabled">
            ...
          </span>
        );
      } else if (currentPage >= 3 || currentPage <= 3) {
        pagination.push(
          <span key="ellipsis" className="btn join-item btn-disabled">
            ...
          </span>
        );
      }

      for (let i = Math.max(totalPages - 2, 4); i <= totalPages; i++) {
        pagination.push(
          <button
            key={i}
            className={`join-item btn ${currentPage === i ? "btn-primary" : ""
              }`}
            onClick={() => handlePageClick(i)}
          >
            {i}
          </button>
        );
      }

      if (currentPage < totalPages) {
        pagination.push(
          <button
            key="next"
            className="join-item btn"
            onClick={() => handlePageClick(currentPage + 1)}
          >
            {t.translations.NEXT}
          </button>
        );
      }
    }

    return pagination;
  };

  return (
    <div
      className={`overflow-x-auto ${bordered ? "rounded-box border border-neutral-content" : ""
        } p-2`}
    >
      <div className="flex justify-between items-center">
        {searchBar && (
          <SearchInput
            placeholder={filterPlaceholder}
            onChange={(e) => setFilterText(e.target.value)}
          />
        )}
        {actionButtons && (
          <div className="p-2">
            <button className="mr-2 text-secondary">
              <FolderIcon className="size-6" />
            </button>
            <button className="mr-2 text-secondary">
              <PresentationChartLineIcon className="size-6" />
            </button>
            <button
              onClick={deleteSelectedRows}
              className={!isAnyRowSelected ? "text-base-100" : "text-accent"}
            >
              <TrashIcon className="size-6" />
            </button>
          </div>
        )}
      </div>
      <table
        className={`table table-pin-cols ${bordered ? "table-bordered" : ""} ${tableClassName ?? ""
          }`}
      >
        <thead>
          <tr className="text-info-content">
            {columns.map((column, index) => (
              <th
                key={index}
                className={`${gridView ? "border border-base-200 bg-info/30" : ""
                  } ${column.sortable !== false ? "cursor-pointer select-none" : ""
                  } ${column.data === "id" ? "sticky left-0 z-10 bg-info-80" : ""
                  }`}
                onClick={() => {
                  if (column.sortable == false || !column.data) return;
                  const direction =
                    sortConfig?.key === column.data &&
                      sortConfig?.direction === "asc"
                      ? "desc"
                      : "asc";
                  setSortConfig({ key: column.data as keyof T, direction });
                }}
              >
                <div className="flex items-center gap-1">
                  {column.header}
                  {sortConfig?.key === column.data &&
                    column.sortable !== false && (
                      <>
                        {sortConfig?.direction === "asc" ? (
                          <ChevronUpIcon className="size-5" />
                        ) : (
                          <ChevronDownIcon className="size-5" />
                        )}
                      </>
                    )}
                </div>
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="text-info-content">
          {currentData.map((row, rowIndex) => {
            const isPrivate = row["visibility" as keyof T] === "Private";
            const rowId = row["id" as keyof T];
            const key =
              typeof rowId === "string" || typeof rowId === "number"
                ? rowId
                : rowIndex;
            return (
              <tr
                key={rowIndex}
                className={`text-info-content ${typeof rowClassName === "function"
                  ? rowClassName(row, rowIndex)
                  : rowClassName || ""
                  } ${isPrivate
                    ? "printer-events-none opacity-60"
                    : "hover:bg-base-200 bg-base-100"
                  }`}
              >
                {columns.map((column, colIndex) => (
                  <td
                    key={colIndex}
                    className={`${column.data === "id"
                      ? "sticky left-0 z-10 bg-info-80"
                      : ""
                      } ${gridView ? "border border-base-200" : ""
                      } text-info-content`}
                  >
                    {column.cell
                      ? column.cell(row)
                      : (row[column.data as keyof T] as React.ReactNode)}
                  </td>
                ))}
                {isPrivate && (
                  <td
                    className="text-right pr-4"
                    title="Private - request access"
                  >
                    <LockClosedIcon className="size-6" />
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
      {enablePagination && filteredData.length > rowsPerPage && (
        <div className="flex justify-center p-2">
          <div className="join">{createPagination()}</div>
        </div>
      )}
    </div>
  );
};

export default GenericTable;
