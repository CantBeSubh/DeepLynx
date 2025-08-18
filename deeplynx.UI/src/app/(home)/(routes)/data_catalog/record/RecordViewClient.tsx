"use client";

import { useState, useMemo } from "react";
import toast from "react-hot-toast";
import Tabs from "@/app/(home)/components/Tabs";
import PropertyTable from "./PropertyTable";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { updateRecord } from "@/app/lib/record_services.client"; // client-safe API call

type Props = {
  initialRecord: FileViewerTableRow | null;
  projectId: number;
  recordId: number;
};

export default function RecordViewClient({
  initialRecord,
  projectId,
  recordId,
}: Props) {
  const [record, setRecord] = useState<FileViewerTableRow | null>(
    initialRecord
  );

  function formatDate(date?: string | null) {
    if (!date) return "N/A";
    return new Date(date).toLocaleDateString("en-US", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
  }

  function renderTags(tags: string | null | undefined) {
    if (!tags) return null;
    try {
      const parsed: string[] = JSON.parse(tags);
      return parsed
        .filter((t) => t != null)
        .map((t) => (
          <span key={t} className="badge mr-1">
            {t}
          </span>
        ));
    } catch {
      return null;
    }
  }

  const systemPropertiesRows = useMemo(
    () => [
      {
        label: "Record Name",
        value: record?.name,
        editable: true,
        onEdit: async (newValue: string) => {
          try {
            const updated = await updateRecord(projectId, recordId, {
              name: newValue,
            });
            setRecord(updated);
            toast.success("Record name updated");
          } catch {
            toast.error("Failed to update record name");
          }
        },
      },
      {
        label: "Record Description",
        value: record?.description,
        editable: true,
        onEdit: async (newValue: string) => {
          try {
            const updated = await updateRecord(projectId, recordId, {
              description: newValue,
            });
            setRecord(updated);
            toast.success("Record description updated");
          } catch {
            toast.error("Failed to update description");
          }
        },
      },
      { label: "Data Source Name", value: record?.dataSourceName },
      { label: "uri", value: record?.uri },
      {
        label: "ClassName",
        value: record?.className,
        // editable example if backend supports class_name update:
        onEdit: async (newValue: string) => {
          try {
            const updated = await updateRecord(projectId, recordId, {
              class_name: newValue,
            });
            setRecord(updated);
            toast.success("Class name updated");
          } catch {
            toast.error("Failed to update class name");
          }
        },
      },
      { label: "OriginalID", value: record?.originalId },
      { label: "Created By", value: record?.createdBy },
      { label: "Created At", value: formatDate(record?.createdAt) },
      { label: "Modified By", value: record?.modifiedBy },
      { label: "Modified At", value: formatDate(record?.modifiedAt) },
    ],
    [record, projectId, recordId]
  );

  const additionalPropertiesRows = useMemo(() => {
    if (!record?.properties) return [];
    try {
      const parsed = JSON.parse(record.properties) as Record<string, unknown>;
      return Object.keys(parsed).map((key) => {
        const value = parsed[key];
        return {
          label: key,
          value:
            typeof value === "object"
              ? JSON.stringify(value)
              : String(value ?? ""),
        };
      });
    } catch {
      return [];
    }
  }, [record?.properties]);

  const tabs = useMemo(
    () => [
      {
        label: "Record Information",
        content: (
          <div className="flex w-full gap-4 mt-4">
            <div className="w-full md:w-1/2 p-3 px-4">
              <PropertyTable
                title="System Properties"
                rows={systemPropertiesRows}
              />
              <PropertyTable
                className="mt-4"
                title="Additional Properties"
                rows={additionalPropertiesRows}
              />
            </div>
            <div className="flex-grow">
              <div className="card bg-base-100 shadow-md p-2">
                <h2 className="card-title">Tags: {renderTags(record?.tags)}</h2>
              </div>
            </div>
          </div>
        ),
      },
      { label: "Timeseries Viewer", content: "" },
      { label: "Graph Viewer", content: "" },
      { label: "Record History", content: "" },
    ],
    [systemPropertiesRows, additionalPropertiesRows, record?.tags]
  );

  return (
    <div className="">
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
        <h1 className="text-2xl font-bold text-info-content">{record?.name}</h1>
      </div>

      <Tabs tabs={tabs} className="p-6" />
    </div>
  );
}
