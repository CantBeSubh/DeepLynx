import React, { useState } from "react";
import SearchInput from "./SearchInput";
import DeleteIcon from "@mui/icons-material/Delete";
import TimelineIcon from "@mui/icons-material/Timeline";
import DriveFileMoveOutlineIcon from "@mui/icons-material/DriveFileMoveOutline";
import { TableRow, Column } from "@/app/(home)/types/types";
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
  rowClassName?: string;
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
}: GenericTableProps<T>) => {
  const [filterText, setFilterText] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [showFullPagination, setShowFullPagination] = useState(false);

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
    const pages: (number | "...")[] = [];

<<<<<<< HEAD:deeplynx.UI/deeplynx-v3/src/app/components/GenericTable.tsx
    const showPageButton = (page: number) => (
      <button
        key={page}
        className={`join-item btn ${
          currentPage === page ? "btn-secondary text-neutral-content" : ""
        }`}
        onClick={() => handlePageClick(page)}
      >
        {page}
      </button>
    );

    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
=======
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
>>>>>>> origin/develop:deeplynx.UI/deeplynx-v3/src/app/(home)/components/GenericTable.tsx
    } else {
      pages.push(1, 2);

<<<<<<< HEAD:deeplynx.UI/deeplynx-v3/src/app/components/GenericTable.tsx
      if (currentPage > 4) pages.push("...");
=======
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
>>>>>>> origin/develop:deeplynx.UI/deeplynx-v3/src/app/(home)/components/GenericTable.tsx

      const middlePages = [
        currentPage - 1,
        currentPage,
        currentPage + 1,
      ].filter((p) => p > 2 && p < totalPages - 1);
      pages.push(...middlePages);

<<<<<<< HEAD:deeplynx.UI/deeplynx-v3/src/app/components/GenericTable.tsx
      if (currentPage < totalPages - 3) pages.push("...");
=======
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
>>>>>>> origin/develop:deeplynx.UI/deeplynx-v3/src/app/(home)/components/GenericTable.tsx

      pages.push(totalPages - 1, totalPages);
    }

    return showFullPagination ? (
      <div className="join">
        {/* Prev Arrow */}
        <button
          className="join-item btn"
          disabled={currentPage === 1}
          onClick={() => handlePageClick(currentPage - 1)}
        >
          «
        </button>

        {/* Page Buttons */}
        {pages.map((page, index) =>
          page === "..." ? (
            <button
              key={`ellipsis-${index}`}
              className="join-item btn btn-disabled text-base-200 bg-accent border-base-200"
            >
              ...
            </button>
          ) : (
            showPageButton(page)
          )
        )}

        {/* Next Arrow */}
        <button
          className="join-item btn"
          disabled={currentPage === totalPages}
          onClick={() => handlePageClick(currentPage + 1)}
        >
          »
        </button>
      </div>
    ) : (
      <div className="join">
        <button
          className="join-item btn"
          disabled={currentPage === 1}
          onClick={() => handlePageClick(currentPage - 1)}
        >
          «
        </button>
        <button className="join-item btn btn-disabled text-neutral border-base-200">{`Page ${currentPage}`}</button>
        <button
          className="join-item btn"
          disabled={currentPage === totalPages}
          onClick={() => handlePageClick(currentPage + 1)}
        >
          »
        </button>
      </div>
    );
  };

  return (
    <div
      className={`overflow-x-auto ${bordered ? "rounded-box border border-neutral-content" : ""
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
            <button className="mr-2 text-secondary-content">
              <DriveFileMoveOutlineIcon fontSize="medium" />
            </button>
            <button className="mr-2 text-secondary-content">
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
        className={`table ${rowClassName} ${bordered ? "table-bordered" : ""}`}
      >
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th
                key={index}
<<<<<<< HEAD:deeplynx.UI/deeplynx-v3/src/app/components/GenericTable.tsx
                className={`text-secondary-content ${
                  column.sortable !== false ? "cursor-pointer select-none" : ""
                }`}
=======
                className={`text-base-content ${column.sortable !== false ? "cursor-pointer select-none" : ""
                  }`}
>>>>>>> origin/develop:deeplynx.UI/deeplynx-v3/src/app/(home)/components/GenericTable.tsx
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
          {currentData.map((row, rowIndex) => (
            <tr
              key={rowIndex}
              className={`hover:bg-base-200/30 bg-base-100 mb-4`}
            >
              {columns.map((column, colIndex) => (
                <td key={colIndex} className="text-secondary-content">
                  {column.cell
                    ? column.cell(row)
                    : (row[column.data as keyof T] as React.ReactNode)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
      {enablePagination && filteredData.length > rowsPerPage && (
        <div className="flex flex-col md:flex-row md:justify-between items-center p-2 gap-2">
          <div className="text-sm text-secondary-content">{`Showing ${
            (currentPage - 1) * rowsPerPage + 1
          } to ${Math.min(currentPage * rowsPerPage, filteredData.length)} of ${
            filteredData.length
          } results`}</div>

          <div className="flex flex-col items-end gap-1">
            {createPagination()}
            <button
              onClick={() => setShowFullPagination(!showFullPagination)}
              className="btn btn-sm btn-outline btn-neutral mt-2"
            >
              {showFullPagination ? "Compact View" : "Full View"}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default GenericTable;
