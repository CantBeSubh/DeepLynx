"use client";

import React, { useState } from "react";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import GenericTable from "@/app/(home)/components/GenericTable";
import Tabs from "@/app/(home)/components/Tabs";
import { fileTableData } from "@/app/(home)/dummy_data/data";
import { FileViewerTableRow, TableRow, Column } from "@/app/(home)/types/types";

const FileViewer = () => {
  // State to manage the file table data
  const [tableData, setTableData] =
    useState<FileViewerTableRow[]>(fileTableData);

  // Function to handle changes in the select checkbox for a row
  const handleSelectChange = (id: number) => {
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

  // Define columns for the GenericTable for files
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
            onChange={() => handleSelectChange(row.id as number)} // Handle checkbox change
          />
        </label>
      ),
    },
    {
      header: "ID",
      accessor: "id", // Accessor for ID column
    },
    {
      header: "File Name",
      accessor: "fileName", // Accessor for file name column
    },
    {
      header: "Timeseries",
      accessor: "timeseries", // Accessor for timeseries column
      cell: (row: TableRow) => {
        const isChecked = "timeseries" in row ? row.timeseries : false; // Check if the row has timeseries data
        return (
          <input
            type="checkbox"
            checked={isChecked} // Control checkbox based on timeseries status
            className="checkbox checkbox-primary"
            readOnly // Make checkbox read-only
          />
        );
      },
    },
    {
      header: "File Size (KB)",
      accessor: "fileSize", // Accessor for file size column
    },
    {
      header: "Date Modified",
      accessor: "dateModified", // Accessor for date modified column
    },
  ];

  // Define columns for the GenericTable for timeseries data
  const timeseries_columns: Column[] = [
    {
      header: "Select",
      accessor: "select", // Accessor for select checkbox
      cell: (row: TableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox"
            checked={row.select || false} // Control checkbox based on row's select status
            onChange={() => handleSelectChange(row.id as number)} // Handle checkbox change
          />
        </label>
      ),
    },
    {
      header: "ID",
      accessor: "id", // Accessor for ID column
    },
    {
      header: "File Name",
      accessor: "fileName", // Accessor for file name column
    },
    {
      header: "File Size (KB)",
      accessor: "fileSize", // Accessor for file size column
    },
    {
      header: "Date Modified",
      accessor: "dateModified", // Accessor for date modified column
    },
  ];

  // Filter to get only rows that are marked as timeseries
  const timeseries_true_info = tableData.filter((row) => row.timeseries);

  /* Tabs data for displaying in the Tabs component */
  const tabData = [
    {
      label: "All", // Label for the first tab
      content: (
        <div>
          <GenericTable
            columns={columns} // Pass columns for the GenericTable
            data={tableData} // Pass the complete table data
            filterPlaceholder="Filter Table..." // Placeholder for filter input
            isAnyRowSelected={isAnyRowSelected} // Check if any row is selected
            deleteSelectedRows={deleteSelectedRows} // Function to delete selected rows
            enablePagination // Enable pagination
          />
        </div>
      ),
    },
    {
      label: "Timeseries", // Label for the second tab
      content: (
        <div>
          <GenericTable
            columns={timeseries_columns} // Pass columns for the timeseries GenericTable
            data={timeseries_true_info} // Pass filtered timeseries data
            isAnyRowSelected={isAnyRowSelected} // Check if any row is selected
            enablePagination={true} // Enable pagination
            rowsPerPage={10} // Set rows per page
            filterPlaceholder="Filter Table..." // Placeholder for filter input
          />
        </div>
      ),
    },
    { label: "Other files", content: <div>Tab content 3</div> }, // Placeholder for third tab
  ];

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-bold">Files</h1> {/* Main title */}
      </div>

      {/* Render Tabs component with tab data */}
      <Tabs tabs={tabData} showButtons={false} />

      {/* Button to add new items, positioned at the bottom right */}
      <button className="fixed bottom-10 right-10">
        <AddCircleIcon className="text-accent" sx={{ fontSize: 60 }} />{" "}
        {/* Add icon */}
      </button>
    </div>
  );
};

export default FileViewer;
