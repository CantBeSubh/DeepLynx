"use client";
import React, { useState } from "react";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import GenericTable from "@/app/(home)/components/GenericTable";
import ModeIcon from "@mui/icons-material/Mode";
import { initialTableData } from "@/app/(home)/dummy_data/data";
import { DataSourceTableRow, TableRow, Column } from "@/app/(home)/types/types";

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
  const isRowAcitve = (obj: TableRow) => ("active" in obj ? obj.active : false);

  // Define columns for the GenericTable
  const columns: Column[] = [
    {
      header: "Select",
      accessor: "select", // Accessor for select checkbox
      cell: (row: TableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox"
            checked={row.select || false} // Control checkbox based on row's select status
            onChange={() => handleSelectChange(row.id as string)} // Handle checkbox change
          />
        </label>
      ),
    },
    {
      header: "Name",
      accessor: "name", // Accessor for name column
    },
    {
      header: "ID",
      accessor: "id", // Accessor for ID column
    },
    {
      header: "Adapter Type",
      accessor: "adapterType", // Accessor for adapter type column
    },
    {
      header: "Active",
      accessor: "active", // Accessor for active status
      cell: (row: TableRow) => (
        <input
          type="checkbox"
          checked={isRowAcitve(row)} // Control checkbox based on row's active status
          className="toggle toggle-primary"
          onChange={() => handleToggleActive(row.id as string)} // Handle active toggle
        />
      ),
    },
    {
      header: "Edit",
      accessor: "edit", // Accessor for edit action
      cell: (row: TableRow) => (
        <button className="btn btn-ghost btn-xs btn-secondary">
          <ModeIcon className="text-accent" /> {/* Edit icon */}
        </button>
      ),
    },
  ];

  return (
    <div>
      <div>
        <div className="flex justify-between items-center mb-4">
          <h1 className="text-2xl font-bold text-base-content">Data Source</h1>
          <i className="text-sm text-base-content">User</i>{" "}
          {/* Placeholder for user info */}
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
          bordered={true} // Add borders to the table
          actionButtons // Enable action buttons
          searchBar // Enable search bar
        />
      </div>
      {/* Button to add new items, positioned at the bottom right */}
      <button className="fixed bottom-10 right-10">
        <AddCircleIcon className="text-accent" fontSize="large" />{" "}
        {/* Add icon */}
      </button>
    </div>
  );
};

export default DataSource;
