"use client";
import React, { useState } from "react";
import DeleteIcon from "@mui/icons-material/Delete";
import { Refresh } from "@mui/icons-material";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import GenericTable from "@/app/components/GenericTable";
import SideMenu from "@/app/components/SideMenu";
import ModeIcon from "@mui/icons-material/Mode";
import Tabs from "@/app/components/Tabs";
import AttachFileIcon from "@mui/icons-material/AttachFile";
import SearchInput from "@/app/components/SearchInput";
import { fileTableData } from "@/app/dummy_data/data";

type TableRow = {
  id: number;
  fileName: string;
  timeseries: boolean;
  fileSize: number;
  dateModified: string;
  select?: boolean;
};

const FileViewer = () => {
  const [tableData, setTableData] = useState<TableRow[]>(fileTableData);

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

  const columns = [
    {
      header: "Select",
      accessor: "select",
      cell: (row: TableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox"
            checked={row.select || false}
            onChange={() => handleSelectChange(row.id)}
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
      cell: (row: TableRow) => (
        <input
          type="checkbox"
          checked={row.timeseries}
          className="checkbox checkbox-primary"
          readOnly
        />
      ),
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

  const timeseries_columns = [
    {
      header: "Select",
      accessor: "select",
      cell: (row: TableRow) => (
        <label>
          <input
            type="checkbox"
            className="checkbox"
            checked={row.select || false}
            onChange={() => handleSelectChange(row.id)}
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

  /* Tabs bummy data */
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

  return (
    <div className="flex bg-base-100">
      <SideMenu />
      <div className="ml-64 flex-1">
        <div className="bg-neutral-content p-4 flex justify-between">
          <h1 className="text-xl font-bold text-secondary-content">
            File Viewer
          </h1>
          <i className="text-sm">User</i>
        </div>
        <div className="p-4">
          <div>
            <Tabs tabs={tabData} showButtons={true} />
          </div>
        </div>
      </div>
      <button className="fixed bottom-10 right-10">
        <AddCircleIcon className="text-primary" fontSize="large" />
      </button>
    </div>
  );
};

export default FileViewer;
