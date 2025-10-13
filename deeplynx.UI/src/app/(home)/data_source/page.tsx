"use client";
import GenericTable from "@/app/(home)/components/GenericTable";
import { initialTableData } from "@/app/(home)/dummy_data/data";
import { Column, DataSourceTableRow, TableRow } from "@/app/(home)/types/types";
import { PencilIcon } from "@heroicons/react/24/outline";
import { PlusCircleIcon } from "@heroicons/react/24/solid";
import { useState } from "react";

const DataSource = () => {
  // State to manage table data
  const [tableData, setTableData] =
    useState<DataSourceTableRow[]>(initialTableData);

  // Function to toggle the active state of a row based on its ID
  const handleToggleActive = (id: string) => {
    setTableData((prevData) =>
      prevData.map(
        (row) => (row.id === id ? { ...row, active: !row.active } : row) // Toggle active status
      )
    );
  };

  // Function to handle changes in the select checkbox for a row
  const handleSelectChange = (id: string) => {
    setTableData((prevData) =>
      prevData.map(
        (row) => (row.id === id ? { ...row, select: !row.select } : row) // Toggle select status
      )
    );
  };

  // Function to delete all selected rows from the table
  const deleteSelectedRows = () => {
    setTableData((prevData) => prevData.filter((row) => !row.select)); // Remove selected rows
  };

  // Check if any row is selected
  const isAnyRowSelected = tableData.some((row) => row.select);
  // Check if a row is active
  const isRowAcitve = (obj: DataSourceTableRow) =>
    "active" in obj ? obj.active : false;

  // Define columns for the GenericTable
  const columns: Column<DataSourceTableRow>[] = [
    {
      header: "Select",
      data: "select", // data for select checkbox
      cell: (row: DataSourceTableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox checkbox-info"
            checked={row.select || false} // Control checkbox based on row's select status
            onChange={() => handleSelectChange(row.id as string)} // Handle checkbox change
          />
        </label>
      ),
    },
    {
      header: "Name",
      data: "name", // data for name column
    },
    {
      header: "ID",
      data: "id", // data for ID column
    },
    {
      header: "Adapter Type",
      data: "adapterType", // data for adapter type column
    },
    {
      header: "Active",
      data: "active", // data for active status
      cell: (row: DataSourceTableRow) => (
        <input
          type="checkbox"
          checked={isRowAcitve(row)} // Control checkbox based on row's active status
          className="toggle text-base-200 checked:text-info"
          onChange={() => handleToggleActive(row.id as string)} // Handle active toggle
        />
      ),
    },
    {
      header: "Edit",
      cell: (row: TableRow) => (
        <button className="btn btn-ghost btn-xs btn-secondary">
          <PencilIcon className="size-6 text-accent" /> {/* Edit icon */}
        </button>
      ),
    },
  ];

  return (
    <div>
      <div>
        <div className="flex justify-between items-center mb-4">
          <h1 className="text-2xl font-bold text-info-content">Data Source</h1>
        </div>

        {/* Render the GenericTable with defined columns and data */}
        <GenericTable
          columns={columns} // Pass columns to the table
          data={tableData} // Pass table data
          filterPlaceholder="Filter Table..." // Placeholder for filter input
          isAnyRowSelected={isAnyRowSelected} // Check if any row is selected
          deleteSelectedRows={deleteSelectedRows} // Function to delete selected rows
          enablePagination={true} // Enable pagination
          rowsPerPage={10} // Set rows per page
          actionButtons // Enable action buttons
          searchBar // Enable search bar
          gridView
        />
      </div>
      {/* Button to add new items, positioned at the bottom right */}
      <button className="fixed bottom-10 right-10">
        <PlusCircleIcon className="size-15 text-accent" />
        {/* Add icon */}
      </button>
    </div>
  );
};

export default DataSource;
