// src/app/(home)/components/PropertyTable.tsx
"use client";

import {
  ArrowDownTrayIcon,
  CheckCircleIcon,
  PencilIcon,
  XCircleIcon,
} from "@heroicons/react/24/outline";
import React, { useState } from "react";
import { useSearchParams } from "next/navigation";
import { downloadFile } from "@/app/lib/file_services.client";

interface PropertyRow {
  label: string;
  value: React.ReactNode;
  editable?: boolean;
  onEdit?: (newValue: string) => void;
}

interface PropertyTableProps {
  title?: string;
  rows: PropertyRow[];
  className?: string;
  download?: boolean;
  recordName?: string;
}

const PropertyTable: React.FC<PropertyTableProps> = ({
  title,
  rows,
  className,
  download = false,
  recordName,
}) => {
  const [editingIdex, setEditingIndex] = useState<number | null>(null);
  const [editValue, setEditValue] = useState<string>("");
  const searchParams = useSearchParams();
  const projectIdParam = searchParams.get("projectId");
  const recordIdParam = searchParams.get("recordId");
  const projectId = projectIdParam ? Number(projectIdParam) : NaN;
  const recordId = recordIdParam ? Number(recordIdParam) : NaN;
  const canDownload = Number.isFinite(projectId) && Number.isFinite(recordId);

  const handleEdit = (index: number, currentValue: string) => {
    setEditingIndex(index);
    setEditValue(currentValue);
  };

  const handleSave = (row: PropertyRow) => {
    row.onEdit?.(editValue);
    setEditingIndex(null);
  };

  const handleCancel = () => {
    setEditingIndex(null);
    setEditValue("");
  };

  return (
    <div className={`${className}`}>
      <div className="card bg-base-100 shadow-md p-2">
        {title && (
          <div className="flex justify-between items-center m-4">
            <h2 className="text-xl font-bold text-base-content">{title}</h2>

            {download && (
              <button
                onClick={() =>
                  canDownload && downloadFile(projectId, recordId, recordName)
                }
                disabled={!canDownload}
                title={
                  canDownload
                    ? "Download file"
                    : "Missing projectId or recordId in URL"
                }
                className={`p-1 transition-colors cursor-pointer ${
                  canDownload
                    ? "hover:text-primary"
                    : "opacity-50 cursor-not-allowed"
                }`}
              >
                <ArrowDownTrayIcon className="w-8 h-8" />
              </button>
            )}
          </div>
        )}

        <div className="card-body p-4">
          <div className="border border-base-300 rounded-lg overflow-hidden bg-base-100">
            {rows.map((row, index) => (
              <div
                key={index}
                className={`grid grid-cols-12 ${
                  index !== rows.length - 1 ? "border-b" : ""
                } border-base-300`}
              >
                <div className="col-span-4 p-3 font-medium text-base-content text-sm bg-base-200 border-r border-base-300">
                  {row.label}
                </div>
                <div className="col-span-7 p-3 text-sm text-base-content break-words">
                  {editingIdex === index ? (
                    <input
                      type="text"
                      value={editValue}
                      onChange={(e) => setEditValue(e.target.value)}
                      className="input input-sm input-bordered w-full"
                    />
                  ) : (
                    <div className="break-words">{row.value}</div>
                  )}
                </div>
                <div className="col-span-1 p-3 flex justify-center items-center">
                  {row.editable && editingIdex !== index && (
                    <PencilIcon
                      className="text-primary hover:text-primary-focus size-6 cursor-pointer transition-colors"
                      onClick={() => handleEdit(index, String(row.value))}
                    />
                  )}
                  {editingIdex === index && (
                    <>
                      <button>
                        <CheckCircleIcon
                          className="text-success hover:text-success-content size-6 cursor-pointer transition-colors"
                          onClick={() => handleSave(row)}
                        />
                      </button>
                      <button>
                        <XCircleIcon
                          className="text-error hover:text-error-content size-6 cursor-pointer transition-colors"
                          onClick={handleCancel}
                        />
                      </button>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PropertyTable;
