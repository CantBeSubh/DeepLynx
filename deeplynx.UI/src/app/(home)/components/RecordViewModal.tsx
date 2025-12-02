"use client";

import React from "react";
import PropertyTable from "./PropertyTable";
import { Column, RecordTableRow } from "../types/types";
import GenericTable from "./GenericTable";
import { RelatedRecordsResponseDto } from "../types/responseDTOs";
import { TagResponseDto } from '../types/responseDTOs';

interface RecordViewModalProps {
  isOpen: boolean;
  onClose: () => void;
  record: RecordTableRow | null;
  relatedRecords: RelatedRecordsResponseDto[];
  tags: TagResponseDto[];
}

interface RelatedRecord {
  relationship: string;
  id: string;
  class: string;
  name: string;
  actions: React.JSX.Element;
}

const RecordViewModal: React.FC<RecordViewModalProps> = ({
  isOpen,
  onClose,
  record,
  relatedRecords,
  tags,
}) => {
  if (!isOpen || !record) return null;

  // Prepare system properties and additional properties for the PropertyTable
  const systemPropertiesRows = [
    { label: "Record Name", value: record.name },
    { label: "Record Description", value: record.description },
    { label: "Data Source Name", value: record.dataSourceName },
    { label: "Uri", value: record.dataSourceName },
    { label: "Class Name", value: record.dataSourceName },
    { label: "OriginalID", value: record.originalId },
    { label: "Last Updated At", value: record.lastUpdatedAt },
  ];

  const relatedRecordsColumns: Column<RelatedRecordsResponseDto>[] = [
    { header: "Relationship", data: "relationshipName" },
    { header: "Name", data: "relatedRecordName" },
  ];

  const parsedProperties = JSON.parse(record.properties!);
  const additionalPropertiesRows = parsedProperties
    ? Object.keys(parsedProperties).map((key) => {
      const value = parsedProperties[key as keyof object];
      return {
        label: key,
        value:
          typeof value === "object" ? JSON.stringify(value) : String(value),
      };
    })
    : [];

  return (
    <dialog className={`modal ${isOpen ? "modal-open" : ""}`}>
      <div className="modal-box max-w-[70vh] max-h-[80vh]">
        <h3 className="font-bold text-2xl mb-4 text-neutral">{record.name}</h3>

        <div className="ml-20 mb-10 mr-20">
          {" "}
          {/* Add margin-bottom for spacing */}
          <PropertyTable
            title="System Properties"
            rows={systemPropertiesRows}
          />
        </div>

        <div className="ml-20 mb-10 mr-20">
          {" "}
          {/* Add margin-bottom for spacing */}
          <PropertyTable
            title="Additional Properties"
            rows={additionalPropertiesRows}
          />
        </div>

        <div className="ml-20 card bg-base-100 shadow-md p-2 relative mb-10 mr-20">
          <div className="mb-4">
            {" "}
            {/* Add margin-bottom for spacing */}
            <h2 className="text-xl font-bold mb-4">Tags:</h2>
            <div className="flex flex-wrap gap-2">
              {" "}
              {/* Use flexbox for better layout */}
              {tags &&
                tags.map((tag: TagResponseDto) => (
                  <span
                    key={tag.id}
                    className="font-inter flex items-center border rounded-full px-2 py-1"
                    style={{ borderColor: "#07519E", color: "#07519E" }}
                  >
                    {tag.name}
                  </span>
                ))}
            </div>
          </div>
        </div>

        <div className="ml-20 mb-4 mr-20">
          {" "}
          {/* Add margin-bottom for spacing */}
          <GenericTable
            columns={relatedRecordsColumns}
            data={relatedRecords} // Ensure this is the correct data structure
            title="Related Records"
            bordered
            searchBar={false}
            enablePagination={false}
            actionButtons={false}
          />
        </div>

        <div className="modal-action">
          <button className="btn" onClick={onClose}>
            Close
          </button>
        </div>
      </div>
    </dialog>
  );
};

function formatDate(date: string | null | undefined): string {
  if (!date) return "N/A";
  return new Date(date).toLocaleDateString("en-US", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}

export default RecordViewModal;
