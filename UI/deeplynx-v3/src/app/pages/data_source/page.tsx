"use client";
import React, { useState } from "react";
import DeleteIcon from "@mui/icons-material/Delete";
import { Refresh } from "@mui/icons-material";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import GenericTable from "@/app/components/GenericTable";
import SideMenu from "@/app/components/SideMenu";
import ModeIcon from "@mui/icons-material/Mode";

type TableRow = {
  name: string;
  country: string;
  adapterType: string;
  active: boolean;
  id: string;
  select?: boolean;
};

const DataSource = () => {
  /* Dummy Data */
  const initialTableData = [
    {
      name: "Alexis Wiley",
      country: "Canada",
      adapterType: "Standard",
      active: true,
      id: "5623",
    },
    {
      name: "Jordan Smith",
      country: "Germany",
      adapterType: "TimeSeries",
      active: false,
      id: "8712",
    },
    {
      name: "Taylor Brown",
      country: "Australia",
      adapterType: "Standard",
      active: true,
      id: "3490",
    },
    {
      name: "Morgan Lee",
      country: "Japan",
      adapterType: "Standard",
      active: false,
      id: "2201",
    },
    {
      name: "Casey Green",
      country: "Brazil",
      adapterType: "TimeSeries",
      active: true,
      id: "4537",
    },
    {
      name: "Dakota Johnson",
      country: "France",
      adapterType: "Standard",
      active: false,
      id: "6345",
    },
    {
      name: "Riley Cooper",
      country: "India",
      adapterType: "TimeSeries",
      active: true,
      id: "7889",
    },
    {
      name: "Quinn Parker",
      country: "Italy",
      adapterType: "Standard",
      active: false,
      id: "9123",
    },
    {
      name: "Avery Morgan",
      country: "South Africa",
      adapterType: "TimeSeries",
      active: true,
      id: "1045",
    },
    {
      name: "Payton Brooks",
      country: "South Korea",
      adapterType: "Standard",
      active: true,
      id: "5567",
    },
    {
      name: "Charlie Taylor",
      country: "Mexico",
      adapterType: "TimeSeries",
      active: false,
      id: "6691",
    },
    {
      name: "Skyler Adams",
      country: "Spain",
      adapterType: "Standard",
      active: true,
      id: "7823",
    },
    {
      name: "Jamie Kelly",
      country: "Netherlands",
      adapterType: "TimeSeries",
      active: false,
      id: "8934",
    },
    {
      name: "Kendall Reed",
      country: "New Zealand",
      adapterType: "Standard",
      active: true,
      id: "9045",
    },
    {
      name: "Hayden Walker",
      country: "Sweden",
      adapterType: "TimeSeries",
      active: true,
      id: "2156",
    },
    {
      name: "Jordan Murphy",
      country: "Norway",
      adapterType: "Standard",
      active: false,
      id: "3267",
    },
    {
      name: "Casey Bailey",
      country: "Denmark",
      adapterType: "TimeSeries",
      active: true,
      id: "4378",
    },
    {
      name: "Morgan Campbell",
      country: "Finland",
      adapterType: "Standard",
      active: true,
      id: "5489",
    },
  ];

  const [tableData, setTableData] = useState<TableRow[]>(initialTableData);

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
          checked={row.active}
          className="toggle toggle-primary"
          onChange={() => handleToggleActive(row.id)}
        />
      ),
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
    <div className="flex bg-base-100">
      <SideMenu />
      <div className="ml-64 flex-1">
        <div className="bg-neutral-content p-4 flex justify-between">
          <h1 className="text-xl font-bold text-secondary-content">
            Data Source
          </h1>
          <i className="text-sm">User</i>
        </div>
        <div className="p-4 flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <label className="input flex items-center">
              <input
                type="text"
                placeholder="Search"
                className="input input-border w-70 pl-10 text-secondary"
              />
              <svg
                className="absolute left-3 top-2.5 w-5 h-5 text-secondary"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  d="M21 21l-4.35-4.35m1.35-5.15a7 7 0 1 1-14 0 7 7 0 0 1 14 0z"
                />
              </svg>
            </label>
            <select
              defaultValue="Pick a color"
              className="select ml-2 p-2 rounded w-70 text-secondary"
            >
              <option disabled={true}>View by</option>
              <option>Crimson</option>
              <option>Amber</option>
              <option>Velvet</option>
            </select>
          </div>
          <div className="flex items-center space-x-4">
            <button
              disabled={!isAnyRowSelected}
              className={!isAnyRowSelected ? "text-base-300" : "text-secondary"}
            >
              <DeleteIcon />
            </button>
            <button
              disabled={!isAnyRowSelected}
              className={!isAnyRowSelected ? "text-base-300" : "text-secondary"}
            >
              <Refresh />
            </button>
            <button>
              <AddCircleIcon className="text-primary" />
            </button>
          </div>
        </div>

        <GenericTable columns={columns} data={tableData} />
      </div>
    </div>
  );
};

export default DataSource;
