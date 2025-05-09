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
  const [tableData, setTableData] =
    useState<DataSourceTableRow[]>(initialTableData);
  const [isMenuCollapsed, setIsMenuCollapsed] = useState<boolean>(false);

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
  const isRowAcitve = (obj: TableRow) => ("active" in obj ? obj.active : false);
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
      cell: (row: TableRow) => (
        <input
          type="checkbox"
          checked={isRowAcitve(row)}
          className="toggle toggle-primary"
          onChange={() => handleToggleActive(row.id as string)}
        />
      ),
    },
    {
      header: "Edit",
      accessor: "edit",
      cell: (row: TableRow) => (
        <button className="btn btn-ghost btn-xs btn-secondary">
          <ModeIcon className="text-accent" />
        </button>
      ),
    },
  ];

  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  return (
    <div className="flex duration-300">
      <SideMenu onToggle={handleMenuToggle} />
      <div
        className={`${
          isMenuCollapsed ? "ml-22" : "ml-64"
        } flex-1 p-6 container mx-auto transition-all duration-300`}
      >
        <div className="flex justify-between items-center mb-4">
          <h1 className="text-2xl font-bold text-base-content">Data Source</h1>
          <i className="text-sm text-base-content">User</i>
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
        <AddCircleIcon className="text-accent" fontSize="large" />
      </button>
    </div>
  );
};

export default DataSource;
