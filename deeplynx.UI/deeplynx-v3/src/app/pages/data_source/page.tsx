"use client";
import React, { useState } from "react";
import DeleteIcon from "@mui/icons-material/Delete";
import { Refresh } from "@mui/icons-material";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import GenericTable from "@/app/components/GenericTable";
import SideMenu from "@/app/components/SideMenu";
import ModeIcon from "@mui/icons-material/Mode";
import { initialTableData } from "@/app/dummy_data/data";
import { DataSourceTableRow, TableRow, Column } from "@/app/types/types";

const DataSource = () => {
  const [tableData, setTableData] = useState<DataSourceTableRow[]>(initialTableData);

  const handleToggleActive = (id: string) => {
    setTableData((prevData) =>
      prevData.map((row) =>
        row.id === id ? { ...row, active: !row.active } : row
      )
    );
  };

  const handleSelectChange = (id: string) => {
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
  const isRowAcitve = (obj: TableRow) => ("active" in obj) ? obj.active : false;
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
            onChange={() => handleSelectChange(row.id as string)}
          />
        </label>
      ),
    },
    {
      header: "Name",
      accessor: "name",
    },
    {
      header: "ID",
      accessor: "id",
    },
    {
      header: "Adapter Type",
      accessor: "adapterType",
    },
    {
      header: "Active",
      accessor: "active",
      cell: (row: TableRow) =>
      (
        <input
          type="checkbox"
          checked={isRowAcitve(row)}
          className="toggle toggle-primary"
          onChange={() => handleToggleActive(row.id as string)}
        />
      )
    },
    {
      header: "Edit",
      accessor: "edit",
      cell: (row: TableRow) => (
        <button className="btn btn-ghost btn-xs btn-secondary">
          <ModeIcon className="text-secondary" />
        </button>
      ),
    },
  ];

  return (
    <div className="flex bg-base-100 duration-300">
      <SideMenu />
      <div className="ml-64 flex-1 p-6 text-base-content">
        <div className="flex justify-between items-center mb-4">
          <h1 className="text-2xl font-bold">Data Source</h1>
          <i className="text-sm">User</i>
        </div>

        <GenericTable
          columns={columns}
          data={tableData}
          filterPlaceholder="Filter Table..."
          isAnyRowSelected={isAnyRowSelected}
          deleteSelectedRows={deleteSelectedRows}
          enablePagination={true}
          rowsPerPage={10}
          bordered={true}
        />
      </div>
      <button className="fixed bottom-10 right-10">
        <AddCircleIcon className="text-primary" fontSize="large" />
      </button>
    </div>
  );
};

export default DataSource;
