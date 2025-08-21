"use client";

import React from "react";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";
import { useRouter } from "next/navigation";
import GenericTable from "./GenericTable";

type GridViewProps = {
  columns: Column<FileViewerTableRow>[];
  data: FileViewerTableRow[];
  activeSearchTerms?: string[];
  selectedProjects?: string[];
};

const GridView = <T extends object>({
  columns,
  data,
  activeSearchTerms = [],
  selectedProjects,
}: GridViewProps) => {
  const router = useRouter();
  const getHighlightedCell = (text: unknown, queries: string[]) => {
    const safeText = String(text);
    if (!queries.length) return { content: safeText, matched: false };

    const lowerText = safeText.toLowerCase();
    const match = queries.find((q) => lowerText.includes(q.toLowerCase()));

    if (!match) return { content: safeText, matched: false };

    const regex = new RegExp(`(${match})`, "gi");
    const parts = safeText.split(regex);

    const content = parts.map((part, index) =>
      regex.test(part) ? (
        <span
          key={index}
          className="font-bold text-info-content bg-info rounded px-1"
        >
          {part}
        </span>
      ) : (
        part
      )
    );
    return { content, matched: true };
  };

  const filteredRecords =
    selectedProjects?.includes("All your Projects") || !selectedProjects
      ? data
      : data.filter(
        (record) =>
          record.projectId !== undefined &&
          selectedProjects.includes(record.projectId?.toString())
      );

  return (
    <div>
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
