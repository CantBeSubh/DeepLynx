import React, { useState } from "react";
import SearchInput from "./SearchInput";
import DeleteIcon from "@mui/icons-material/Delete";
import TimelineIcon from "@mui/icons-material/Timeline";
import DriveFileMoveOutlineIcon from "@mui/icons-material/DriveFileMoveOutline";
import { TableRow, Column } from "@/app/types/types";
import KeyboardArrowUpIcon from "@mui/icons-material/KeyboardArrowUp";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";

type GenericTableProps = {
  columns: Column[];
  data: TableRow[];
  filterPlaceholder?: string;
  isAnyRowSelected?: boolean;
  deleteSelectedRows?: () => void;
  rowsPerPage?: number; // Optional rowsPerPage prop
  enablePagination?: boolean; // Optional enablePagination prop
  bordered?: boolean; // Optional bordered prop
  searchBar?: boolean;
  actionButtons?: boolean;
};

const GenericTable: React.FC<GenericTableProps> = ({
  columns,
  data,
  filterPlaceholder,
  isAnyRowSelected,
  deleteSelectedRows,
  rowsPerPage = 10, // Default rowsPerPage to 10 if not provided
  enablePagination = false, // Default enablePagination to false if not provided
  bordered = false, // Default bordered to false if not provided
  searchBar = false,
  actionButtons = false,
}) => {
  const [filterText, setFilterText] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  const filteredData = data.filter((row) =>
    columns.some((column) =>
      row[column.accessor as keyof typeof row]
        ?.toString()
        .toLowerCase()
        .includes(filterText.toLowerCase())
    )
  );

  // Column Sorting
  const [sortConfig, setSortConfig] = useState<{
    key: string;
    direction: "asc" | "desc";
  } | null>(null);

  const sortedData = React.useMemo(() => {
    if (!sortConfig) return filteredData;

    return [...filteredData].sort((a, b) => {
      const aValue = a[sortConfig.key as keyof TableRow];
      const bValue = b[sortConfig.key as keyof TableRow];

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

  // Calculate total pages if pagination is enabled
  const totalPages = enablePagination
    ? Math.ceil(filteredData.length / rowsPerPage)
    : 1;

  // Get current page data if pagination is enabled
  const currentData = enablePagination
    ? sortedData.slice(
        (currentPage - 1) * rowsPerPage,
        currentPage * rowsPerPage
      )
    : sortedData;

  const handlePageClick = (pageNumber: number) => {
    setCurrentPage(pageNumber);
  };

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
      <table className={`table ${bordered ? "table-bordered" : ""}`}>
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th
                key={index}
                className="text-base-content cursor-pointer select-none"
                onClick={() => {
                  const direction =
                    sortConfig?.key === column.accessor &&
                    sortConfig.direction === "asc"
                      ? "desc"
                      : "asc";
                  setSortConfig({ key: column.accessor, direction });
                }}
              >
                {column.header}
                {sortConfig?.key === column.accessor && (
                  <span>
                    {sortConfig.direction === "asc" ? (
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
            <tr key={rowIndex} className="hover:bg-base-200">
              {columns.map((column, colIndex) => (
                <td key={colIndex} className="text-base-content">
                  {column.cell
                    ? column.cell(row)
                    : row[column.accessor as keyof typeof row]}
                </td>
              ))}
            </tr>
          ))}
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
