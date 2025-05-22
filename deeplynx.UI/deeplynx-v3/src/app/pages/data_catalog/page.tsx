"use client";

import LargeSearchBar from "@/app/components/LargeSearchBar";
import React, { useState, useEffect } from "react";
import { fileTableData } from "@/app/dummy_data/data";
import GenericTable from "@/app/components/GenericTable";
import { Column, TableRow, FileViewerTableRow } from "@/app/types/types";

const DataCatalog = () => {
  const [tableData, setTableData] =
    useState<FileViewerTableRow[]>(fileTableData);
  const [projectName, setProjectName] = useState<string>("");
  const [hasMounted, setHasMounted] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState<number>(1);

  useEffect(() => {
    setHasMounted(true);
    const storedName = localStorage.getItem("selectedProjectName");
    if (storedName) setProjectName(storedName);
  }, []);

  useEffect(() => {
    if (activeFilters.length === 0) {
      // if no filter, show all data
      setTableData(fileTableData);
    } else {
      // apply filters and loggic to show filter results
      const filteredData = fileTableData.filter((file) => {
        return activeFilters.some((filter) =>
          file.fileName.toLowerCase().includes(filter.term.toLowerCase())
        );
      });
      setTableData(filteredData);
    }
  }, [activeFilters]);

  const handleSearch = (value: string) => {
    if (
      !value.trim() ||
      activeFilters.some((filter) => filter.term === value.trim())
    ) {
      return;
    }
    setActiveFilters([
      ...activeFilters,
      { id: nextFilterId, term: value.trim() },
    ]);
    setNextFilterId(nextFilterId + 1);
    setSearchTerm("");
  };

  const removeFilter = (filerId: number) => {
    setActiveFilters(activeFilters.filter((filter) => filter.id !== filerId));
  };

  const clearAllFilters = () => {
    setActiveFilters([]);
    setSearchTerm("");
  };

  const columns: Column[] = [
    {
      header: "File Name",
      accessor: "fileName",
    },
    {
      header: "Explore",
      accessor: "explore",
      cell: (row: TableRow) => (
        <button className="btn btn-primary btn-outline btn-xs">Explore</button>
      ),
    },
  ];

  if (!hasMounted) {
    return null;
  }
  return (
    <div>
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">
            {hasMounted ? `Project Name: ${projectName}` : "Loading..."}
          </h1>
        </div>
      </div>
      <div className="divider"></div>
      <div className="flex justify-center">
        <LargeSearchBar
          onEnter={handleSearch}
          placeholder="Search files by name..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>
      {activeFilters.length > 0 && (
        <div className="max-w-3xl mx-auto mb-6 mt-3">
          <div className="flex flex-wrap items-center gap-2">
            {activeFilters.map((filter) => (
              <div
                key={filter.id}
                className="bg-base-200 rounded-full px-3 py-1 flex items-center gap-2 text-sm"
              >
                <span>Filtered by: {filter.term}</span>
                <button
                  onClick={() => removeFilter(filter.id)}
                  className="hover:text-error"
                >
                  x
                </button>
              </div>
            ))}
            {activeFilters.length > 1 && (
              <button
                onClick={clearAllFilters}
                className="text-sm hover:underline ml-2"
              >
                Clear all
              </button>
            )}
          </div>
        </div>
      )}
      {activeFilters.length > 0 && tableData.length === 0 && (
        <div>
          <p>No results found.</p>
        </div>
      )}
      {activeFilters.length >= 1 && (
        <div className="border-b border-base-200">
          <h2>Found {tableData.length} matches</h2>
        </div>
      )}
      <GenericTable columns={columns} data={tableData} />
    </div>
  );
};

export default DataCatalog;
