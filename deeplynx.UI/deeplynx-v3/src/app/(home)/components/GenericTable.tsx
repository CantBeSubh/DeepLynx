import React, { useState } from "react";
import SearchInput from "./SearchInput";
import DeleteIcon from "@mui/icons-material/Delete";
import TimelineIcon from "@mui/icons-material/Timeline";
import DriveFileMoveOutlineIcon from "@mui/icons-material/DriveFileMoveOutline";
import { Column } from "@/app/(home)/types/types";
import KeyboardArrowUpIcon from "@mui/icons-material/KeyboardArrowUp";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";

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
            className={`join-item btn ${
              currentPage === i ? "btn-primary" : ""
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
            Prev
          </button>
        );
      }

      for (let i = 1; i <= Math.min(3, totalPages); i++) {
        pagination.push(
          <button
            key={i}
            className={`join-item btn ${
              currentPage === i ? "btn-primary" : ""
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
            className={`join-item btn ${
              currentPage === i ? "btn-primary" : ""
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
            Next
          </button>
        );
      }
    }

    return pagination;
  };

  return (
    <div
      className={`overflow-x-auto ${
        bordered ? "rounded-box border border-neutral-content" : ""
      } p-2`}
    >
      <div className="my-4 flex justify-between items-center">
        {searchBar && (
          <SearchInput
            placeholder={filterPlaceholder}
            onChange={(e) => setFilterText(e.target.value)}
          />
        )}
        {actionButtons && (
          <div className="p-2">
            <button className="mr-2 text-secondary">
              <DriveFileMoveOutlineIcon fontSize="medium" />
            </button>
            <button className="mr-2 text-secondary">
              <TimelineIcon fontSize="medium" />
            </button>
            <button
              onClick={deleteSelectedRows}
              className={!isAnyRowSelected ? "text-base-100" : "text-accent"}
            >
              <DeleteIcon fontSize="medium" />
            </button>
          </div>
        )}
      </div>
      <table
        className={`table table-pin-cols ${bordered ? "table-bordered" : ""} ${
          tableClassName ?? ""
        }`}
      >
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th
                key={index}
                className={`text-base-content ${
                  gridView ? "border border-base-200 bg-info/30" : ""
                } ${
                  column.sortable !== false ? "cursor-pointer select-none" : ""
                } ${
                  column.data === "id" ? "sticky left-0 z-10 bg-info-80" : ""
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
                {column.header}
                {sortConfig?.key === column.data &&
                  column.sortable !== false && (
                    <span>
                      {sortConfig?.direction === "asc" ? (
                        <KeyboardArrowUpIcon />
                      ) : (
                        <KeyboardArrowDownIcon />
                      )}
                    </span>
                  )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {currentData.map((row, rowIndex) => {
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
                    ? "printer-events-none opacity-60"
                    : "hover:bg-base-200 bg-base-100"
                }`}
              >
                {columns.map((column, colIndex) => (
                  <td
                    key={colIndex}
                    className={`${
                      column.data === "id"
                        ? "sticky left-0 z-10 bg-info-80"
                        : ""
                    } ${
                      gridView ? "border border-base-200" : ""
                    } text-base-content`}
                  >
                    {column.cell
                      ? column.cell(row)
                      : (row[column.data as keyof T] as React.ReactNode)}
                  </td>
                ))}
                {isPrivate && (
                  <td
                    className="text-right pr-4 text-secondary"
                    title="Private - request access"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                      strokeWidth={1.5}
                      stroke="currentColor"
                      className="size-6"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M16.5 10.5V6.75a4.5 4.5 0 1 0-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 0 0 2.25-2.25v-6.75a2.25 2.25 0 0 0-2.25-2.25H6.75a2.25 2.25 0 0 0-2.25 2.25v6.75a2.25 2.25 0 0 0 2.25 2.25Z"
                      />
                    </svg>
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
