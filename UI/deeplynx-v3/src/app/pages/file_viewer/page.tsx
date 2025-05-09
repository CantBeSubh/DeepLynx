"use client";
import React, { useState } from "react";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import GenericTable from "@/app/components/GenericTable";
import SideMenu from "@/app/components/SideMenu";
import Tabs from "@/app/components/Tabs";
import { fileTableData } from "@/app/dummy_data/data";
import { FileViewerTableRow, TableRow, Column } from "@/app/types/types";

const FileViewer = () => {
  const [tableData, setTableData] =
    useState<FileViewerTableRow[]>(fileTableData);
  const [isMenuCollapsed, setIsMenuCollapsed] = useState<boolean>(false);

  const handleSelectChange = (id: number) => {
    setTableData((prevData) =>
      prevData.map((row) =>
        row.id === id ? { ...row, select: !row.select } : row
      )
    );
  };

  const deleteSelectedRows = () => {
    setTableData((prevData) => prevData.filter((row) => !row.select));
  };

  const isAnyRowSelected = tableData.some((row) => row.select);

  const columns: Column[] = [
    {
      header: "Select",
      accessor: "select",
      cell: (row: TableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox"
            checked={row.select || false}
            onChange={() => handleSelectChange(row.id as number)}
          />
        </label>
      ),
    },
    {
      header: "ID",
      accessor: "id",
    },
    {
      header: "File Name",
      accessor: "fileName",
    },
    {
      header: "Timeseries",
      accessor: "timeseries",
      cell: (row: TableRow) => {
        const isChecked = "timeseries" in row ? row.timeseries : false;
        return (
          <input
            type="checkbox"
            checked={isChecked}
            className="checkbox checkbox-primary"
            readOnly
          />
        );
      },
    },
    {
      header: "File Size (KB)",
      accessor: "fileSize",
    },
    {
      header: "Date Modified",
      accessor: "dateModified",
    },
  ];

  const timeseries_columns: Column[] = [
    {
      header: "Select",
      accessor: "select",
      cell: (row: TableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox"
            checked={row.select || false}
            onChange={() => handleSelectChange(row.id as number)}
          />
        </label>
      ),
    },
    {
      header: "ID",
      accessor: "id",
    },
    {
      header: "File Name",
      accessor: "fileName",
    },
    {
      header: "File Size (KB)",
      accessor: "fileSize",
    },
    {
      header: "Date Modified",
      accessor: "dateModified",
    },
  ];

  const timeseries_true_info = tableData.filter((row) => row.timeseries);

  /* Tabs dummy data */
  const tabData = [
    {
      label: "All",
      content: (
        <div>
          <GenericTable
            columns={columns}
            data={tableData}
            filterPlaceholder="Filter Table..."
            isAnyRowSelected={isAnyRowSelected}
            deleteSelectedRows={deleteSelectedRows}
            enablePagination
          />
        </div>
      ),
    },
    {
      label: "Timeseries",
      content: (
        <div>
          <GenericTable
            columns={timeseries_columns}
            data={timeseries_true_info}
            isAnyRowSelected={isAnyRowSelected}
            enablePagination={true}
            rowsPerPage={10}
            filterPlaceholder="Filter Table..."
          />
        </div>
      ),
    },
    { label: "Other files", content: <div>Tab content 3</div> },
  ];

  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  return (
    <div className="flex bg-base-100">
      <SideMenu onToggle={handleMenuToggle} />
      <div
        className={`${
          isMenuCollapsed ? "ml-22" : "ml-64"
        } flex-1 p-6 container mx-auto transition-all duration-300`}
      >
        <div className="flex justify-between items-center mb-4">
          <h1 className="text-2xl font-bold">Files</h1>
          <i className="text-sm">User</i>
        </div>
        <div className="p-4">
          <div>
            <Tabs tabs={tabData} showButtons={false} />
          </div>
        </div>
      </div>
      <button className="fixed bottom-10 right-10">
        <AddCircleIcon className="text-accent" sx={{ fontSize: 60 }} />
      </button>
    </div>
  );
};

export default FileViewer;
