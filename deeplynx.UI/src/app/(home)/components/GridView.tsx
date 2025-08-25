"use client";

import React from "react";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";
import GenericTable from "./GenericTable";

type GridViewProps = {
  columns: Column<FileViewerTableRow>[];
  data: FileViewerTableRow[];
  activeSearchTerms?: string[];
  selectedProjects?: string[];
};

const GridView = ({
  columns,
  data,
  selectedProjects,
}: GridViewProps) => {

  const filteredRecords =
    selectedProjects?.includes("All your Projects") || !selectedProjects
      ? data
      : data.filter(
        (record) =>
          record.projectId !== undefined &&
          selectedProjects.includes(record.projectId?.toString())
      );

  return (
    <div className="p-8">
      <GenericTable
        columns={columns}
        data={filteredRecords}
        enablePagination
        rowsPerPage={8}
        gridView={true}
      />
    </div>
  );
};

export default GridView;
