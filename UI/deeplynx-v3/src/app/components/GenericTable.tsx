import React, { useState } from "react";
import SearchInput from "./SearchInput";
import DeleteIcon from "@mui/icons-material/Delete";
import TimelineIcon from "@mui/icons-material/Timeline";
import DriveFileMoveOutlineIcon from "@mui/icons-material/DriveFileMoveOutline";

type Column = {
  header: string;
  accessor: string;
  cell?: (row: any) => React.ReactNode;
};

type GenericTableProps = {
  columns: Column[];
  data: any[];
  filterPlaceholder?: string;
  isAnyRowSelected?: boolean;
  deleteSelectedRows?: () => void;
  rowsPerPage?: number; // Optional rowsPerPage prop
  enablePagination?: boolean; // Optional enablePagination prop
  bordered?: boolean; // Optional bordered prop
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
}) => {
  const [filterText, setFilterText] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  const filteredData = data.filter((row) =>
    columns.some((column) =>
      row[column.accessor]
        ?.toString()
        .toLowerCase()
        .includes(filterText.toLowerCase())
    )
  );

  // Calculate total pages if pagination is enabled
  const totalPages = enablePagination
    ? Math.ceil(filteredData.length / rowsPerPage)
    : 1;

  // Get current page data if pagination is enabled
  const currentData = enablePagination
    ? filteredData.slice(
        (currentPage - 1) * rowsPerPage,
        currentPage * rowsPerPage
      )
    : filteredData;

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
        <SearchInput
          placeholder={filterPlaceholder}
          onChange={(e) => setFilterText(e.target.value)}
        />
        <div className="p-2">
          <button className="mr-2 text-secondary">
            <DriveFileMoveOutlineIcon fontSize="medium" />
          </button>
          <button className="mr-2 text-secondary">
            <TimelineIcon fontSize="medium" />
          </button>
          <button
            onClick={deleteSelectedRows}
            className={!isAnyRowSelected ? "text-base-300" : "text-secondary"}
          >
            <DeleteIcon fontSize="medium" />
          </button>
        </div>
      </div>
      <table className={`table ${bordered ? "table-bordered" : ""}`}>
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th key={index}>{column.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {currentData.map((row, rowIndex) => (
            <tr key={rowIndex} className="hover:bg-secondary">
              {columns.map((column, colIndex) => (
                <td key={colIndex} className="text-primary-content">
                  {column.cell ? column.cell(row) : row[column.accessor]}
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
