"use client";

import { Column } from "@/app/(home)/types/types";
import { useLanguage } from "@/app/contexts/Language";
import {
  ChevronDownIcon,
  ChevronUpIcon,
  FolderIcon,
  LockClosedIcon,
  PresentationChartLineIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import React from "react";
import SearchInput from "./SearchInput";

// Pagination metadata from server
export type PaginationInfo = {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
};

// Define the props for ServerPaginatedTable
type ServerPaginatedTableProps<T extends object> = {
  columns: Column<T>[];
  data: T[];
  paginationInfo: PaginationInfo;
  onPageChange: (pageNumber: number) => void;
  title?: string;
  isAnyRowSelected?: boolean;
  deleteSelectedRows?: () => void;
  bordered?: boolean;
  searchBar?: boolean;
  filterPlaceholder?: string;
  onSearchChange?: (searchText: string) => void;
  actionButtons?: boolean;
  rowClassName?: string | ((row: T, index: number) => string);
  tableClassName?: string;
  gridView?: boolean;
  pageSizeOptions?: number[];
  onPageSizeChange?: (pageSize: number) => void;
};

const ServerPaginatedTable = <T extends object>({
  columns,
  data,
  paginationInfo,
  onPageChange,
  title,
  isAnyRowSelected,
  deleteSelectedRows,
  bordered = false,
  searchBar = false,
  filterPlaceholder,
  onSearchChange,
  actionButtons = false,
  rowClassName,
  tableClassName,
  gridView = false,
  pageSizeOptions = [10, 25, 50, 100],
  onPageSizeChange,
}: ServerPaginatedTableProps<T>) => {
  const { t } = useLanguage();

  const { pageNumber, pageSize, totalCount, totalPages, hasPrevious, hasNext } =
    paginationInfo;

  // Handle page click
  const handlePageClick = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPages) {
      onPageChange(newPage);
    }
  };

  // Create pagination buttons
  const createPagination = () => {
    const pagination = [];

    if (totalPages <= 6) {
      for (let i = 1; i <= totalPages; i++) {
        pagination.push(
          <button
            key={i}
            className={`join-item btn ${pageNumber === i ? "btn-primary" : ""}`}
            onClick={() => handlePageClick(i)}
          >
            {i}
          </button>
        );
      }
    } else {
      if (hasPrevious) {
        pagination.push(
          <button
            key="prev"
            className="join-item btn"
            onClick={() => handlePageClick(pageNumber - 1)}
          >
            {t.translations.PREV}
          </button>
        );
      }

      for (let i = 1; i <= Math.min(3, totalPages); i++) {
        pagination.push(
          <button
            key={i}
            className={`join-item btn ${pageNumber === i ? "btn-primary" : ""}`}
            onClick={() => handlePageClick(i)}
          >
            {i}
          </button>
        );
      }

      if (pageNumber > 3 && pageNumber <= totalPages - 3) {
        pagination.push(
          <span key="ellipsis1" className="btn join-item btn-disabled">
            ...
          </span>
        );
        pagination.push(
          <button
            key={pageNumber}
            className="join-item btn btn-primary"
            onClick={() => handlePageClick(pageNumber)}
          >
            {pageNumber}
          </button>
        );
        pagination.push(
          <span key="ellipsis2" className="btn join-item btn-disabled">
            ...
          </span>
        );
      } else if (pageNumber >= 3 || pageNumber <= 3) {
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
            className={`join-item btn ${pageNumber === i ? "btn-primary" : ""}`}
            onClick={() => handlePageClick(i)}
          >
            {i}
          </button>
        );
      }

      if (hasNext) {
        pagination.push(
          <button
            key="next"
            className="join-item btn"
            onClick={() => handlePageClick(pageNumber + 1)}
          >
            {t.translations.NEXT}
          </button>
        );
      }
    }

    return pagination;
  };

  // Calculate current range
  const startRecord = Math.min((pageNumber - 1) * pageSize + 1, totalCount);
  const endRecord = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div
      className={`overflow-x-auto ${
        bordered ? "rounded-box border border-base-300" : ""
      } p-4`}
    >
      {title && (
        <h2 className="text-xl font-bold text-base-content">{title}</h2>
      )}
      
      <div className="my-2 flex justify-between items-center">
        {searchBar && (
          <SearchInput
            placeholder={filterPlaceholder}
            onChange={(e) => onSearchChange?.(e.target.value)}
          />
        )}
        {actionButtons && (
          <div className="p-2">
            <button className="mr-2 text-primary hover:text-primary-focus transition-colors">
              <FolderIcon className="size-6" />
            </button>
            <button className="mr-2 text-primary hover:text-primary-focus transition-colors">
              <PresentationChartLineIcon className="size-6" />
            </button>
            <button
              onClick={deleteSelectedRows}
              className={`transition-colors ${
                !isAnyRowSelected
                  ? "text-base-300 cursor-not-allowed"
                  : "text-error hover:text-error-focus cursor-pointer"
              }`}
              disabled={!isAnyRowSelected}
            >
              <TrashIcon className="size-6" />
            </button>
          </div>
        )}
      </div>

      {/* Pagination info and page size selector */}
      <div className="flex justify-between items-center mb-2">
        <div className="text-sm text-base-content/70">
          Showing {startRecord} - {endRecord} of {totalCount} results
        </div>
        {onPageSizeChange && (
          <div className="flex items-center gap-2">
            <label className="text-sm text-base-content/70 w-56">Rows per page:</label>
            <select
              className="select select-bordered select-sm"
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
            >
              {pageSizeOptions.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>
        )}
      </div>

      <table
        className={`table table-pin-cols ${bordered ? "table-bordered" : ""} ${
          tableClassName ?? ""
        }`}
      >
        <thead>
          <tr className="text-base-content bg-base-300">
            {columns.map((column, index) => (
              <th
                key={index}
                className={`${
                  gridView ? "border border-base-300 bg-base-200" : ""
                } ${column.data === "id" ? "sticky left-0 z-10 bg-base-300" : ""}`}
              >
                <div className="flex items-center gap-1">
                  {column.header}
                </div>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="text-center py-8 text-base-content/50">
                No data available
              </td>
            </tr>
          ) : (
            data.map((row, rowIndex) => {
              const isPrivate = row["visibility" as keyof T] === "Private";
              const rowId = row["id" as keyof T];
              const key =
                typeof rowId === "string" || typeof rowId === "number"
                  ? rowId
                  : rowIndex;
              return (
                <tr
                  key={key}
                  className={`${
                    typeof rowClassName === "function"
                      ? rowClassName(row, rowIndex)
                      : rowClassName || ""
                  } ${
                    isPrivate
                      ? "opacity-60 cursor-not-allowed"
                      : "hover:bg-base-200 transition-colors"
                  }`}
                >
                  {columns.map((column, colIndex) => (
                    <td
                      key={colIndex}
                      className={`text-base-content ${
                        column.data === "id"
                          ? "sticky left-0 z-10 bg-base-100"
                          : ""
                      } ${gridView ? "border border-base-300" : ""}`}
                    >
                      {column.cell
                        ? column.cell(row, rowIndex)
                        : (row[column.data as keyof T] as React.ReactNode)}
                    </td>
                  ))}
                  {isPrivate && (
                    <td
                      className="text-right pr-4"
                      title="Private - request access"
                    >
                      <LockClosedIcon className="size-6 text-warning" />
                    </td>
                  )}
                </tr>
              );
            })
          )}
        </tbody>
      </table>

      {/* Pagination controls */}
      {totalPages > 1 && (
        <div className="flex justify-center p-2 mt-4">
          <div className="join">{createPagination()}</div>
        </div>
      )}
    </div>
    
  );
};

export default ServerPaginatedTable;