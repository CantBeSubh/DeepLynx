"use client";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import { useEffect, useState } from "react";

import GenericTable from "@/app/(home)/components/GenericTable";
import { fileTableData } from "@/app/(home)/dummy_data/data";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { useRouter } from "next/navigation";
import ListView from "./ListView";

const DataCatalog = () => {
  const router = useRouter();
  // State to manage table data, project name, mount status, search term, active filters, and next filter ID
  const [tableData, setTableData] =
    useState<FileViewerTableRow[]>(fileTableData);
  const [projectName, setProjectName] = useState<string>("");
  const [hasMounted, setHasMounted] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [activeFilters, setActiveFilters] = useState<
    Array<{ id: number; term: string }>
  >([]);
  const [nextFilterId, setNextFilterId] = useState<number>(1);

  // Effect to set project name from localStorage and mark the component as mounted
  useEffect(() => {
    setHasMounted(true);
    const storedName = localStorage.getItem("selectedProjectName");
    if (storedName) setProjectName(storedName);
  }, []);

  useEffect(() => {
    if (activeFilters.length === 0) {
      setTableData(fileTableData);
    } else {
      const filtered = fileTableData.filter((row) => {
        return activeFilters.some((filter) => {
          const query = filter.term.toLowerCase();
          return (
            row.fileName.toLowerCase().includes(query) ||
            row.fileDescription.toLowerCase().includes(query) ||
            row.fileType.toLowerCase().includes(query) ||
            row.tags.some((tag) => tag.toLowerCase().includes(query)) ||
            (row.timeseries ? "yes" : "no").includes(query) ||
            row.lastEdit.toLowerCase().includes(query)
          );
        });
      });
      setTableData(filtered);
    }
  }, [activeFilters]);

  // Function to handle search input and add it as an active filter
  const handleSearch = (value: string) => {
    if (
      !value.trim() || // Ignore empty searches
      activeFilters.some((filter) => filter.term === value.trim()) // Ignore duplicates
    ) {
      return;
    }
    setActiveFilters([
      ...activeFilters,
      { id: nextFilterId, term: value.trim() }, // Add new filter
    ]);
    setNextFilterId(nextFilterId + 1); // Increment filter ID for next filter
    setSearchTerm(""); // Clear search input
  };

  // Function to remove a specific filter by its ID
  const removeFilter = (filterId: number) => {
    setActiveFilters(activeFilters.filter((filter) => filter.id !== filterId));
  };

  // Function to clear all active filters
  const clearAllFilters = () => {
    setActiveFilters([]); // Reset filters
    setSearchTerm(""); // Clear search input
  };

  const activeSearchTerms = activeFilters.map((f) => f.term.toLowerCase());

  // If the component has not mounted yet, return null to avoid rendering
  if (!hasMounted) {
    return null;
  }

  return (
    <div>
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-secondary-content">
            Data Catalog
          </h1>
        </div>
      </div>
      <div className="divider"></div>
      <LargeSearchBar
        placeholder="Seach records ..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        onEnter={handleSearch}
        activeFilters={activeFilters}
        onRemoveFilter={removeFilter}
        onClearAll={clearAllFilters}
        resultCount={tableData.length}
        showResultsMessage={activeFilters.length > 0}
      />
      <ListView records={tableData} activeSearchTerms={activeSearchTerms} />
    </div>
  );
};

export default DataCatalog;
