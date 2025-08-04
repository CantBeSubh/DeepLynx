"use client";

import GenericTable from "@/app/(home)/components/GenericTable";
import Tabs from "@/app/(home)/components/Tabs";
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { getRecord, updateRecord } from "@/app/lib/record_services";
import { useSearchParams } from "next/navigation";
import React, { Suspense, useEffect, useState } from "react";
import PropertyTable from "./PropertyTable";
import ExpandableTagsCell from "../ExpandableTagCell";
import toast from "react-hot-toast";

const RecordViewPageContent = () => {
  const params = useSearchParams();
  const recordId = params.get("recordId");
  const projectId = params.get("projectId");

  const [record, setRecord] = useState<FileViewerTableRow | null>(null);

  useEffect(() => {
    if (!recordId || !projectId) return;

    console.log("RecordId after:", recordId);
    console.log("ProjectId after:", projectId);
    const fetchData = async () => {
      try {
        const data = await getRecord(Number(projectId), Number(recordId));
        setRecord(data);
      } catch (error) {
        console.error("Error fetching record:", error);
      }
    };
    fetchData();
  }, [recordId, projectId]);

  const renderTags = (tags: string) => {
    try {
      const parsedTags: string[] = JSON.parse(tags);
      return parsedTags
        .filter((t: string) => t !== null && t !== undefined)
        .map((t: string) => (
          <span key={t} className="badge mr-1">
            {t}
          </span>
        ));
    } catch {
      return null;
    }
  };

  if (!record) {
    return <div className="loading loading-spinner loading-xl" />;
  }

  const systemPropertiesRows = [
    {
      label: "Project",
      value: record?.name,
      editable: true,
      onEdit: async (newValue: string) => {
        try {
          const update = await updateRecord(
            Number(projectId),
            Number(recordId),
            {
              name: newValue,
            }
          );
          setRecord(update);
          toast.success("Project name updated");
        } catch (error) {
          toast.error("Failed to update project name");
        }
      },
    },
    {
      label: "Data Source Name",
      value: record.dataSourceName,
    },
    {
      label: "Record Description",
      value: record.description,
      editable: true,
      onEdit: async (newValue: string) => {
        try {
          const update = await updateRecord(
            Number(projectId),
            Number(recordId),
            {
              description: newValue,
            }
          );
          setRecord(update);
          toast.success("Record description updated");
        } catch (error) {
          toast.error("Failed to update Record description");
        }
      },
    },
    { label: "uri", value: record.uri },
    {
      label: "ClassName",
      value: record.className,
      // editable: true, I dont see a className being returned form the BE
      onEdit: async (newValue: string) => {
        try {
          const update = await updateRecord(
            Number(projectId),
            Number(recordId),
            {
              class_name: newValue,
            }
          );
          setRecord(update);
          toast.success("Class Name updated");
        } catch (error) {
          toast.error("Failed to update Class Name");
        }
      },
    },
    { label: "OriginalID", value: record.originalId },
    { label: "Created By", value: record.createdBy },
    { label: "Created At", value: formatDate(record.createdAt) },
    { label: "Modified By", value: record.modifiedBy },
    { label: "Modified At", value: formatDate(record.modifiedAt) },
  ];

  // TODO: make nested objects in table form too
  const parsedProperties = JSON.parse(record.properties!);
  const additionalPropertiesRows = parsedProperties
    ? Object.keys(parsedProperties).map(key => {
        const value = parsedProperties[key as keyof object];
        return {
          label: key,
          value: typeof value === 'object' ? JSON.stringify(value) : value,
        };
      })
    : [];

  const tabs = [
    {
      label: "Record Information",
      content: (
        <div className="flex gap-4">
          <div className="w-full md:w-1/2 p-3">
            <PropertyTable
              title="System Properties"
              rows={systemPropertiesRows}
            />
            <PropertyTable
              title="Additional Properties"
              rows={additionalPropertiesRows}
            />
          </div>
          <div className="flex-grow">
            <div className="card bg-base-100 shadow-md p-2">
              <h2 className="card-title">Tags: {renderTags(record.tags)}</h2>
            </div>

            {/* <div className="card bg-base-100 shadow-md p-2 ">
              <div className="card-body">
                <h2 className="card-title">
                  Tags:{" "}
                  {record.tags.map((tag) => (
                    <div className="card-actions">
                      <div className="badge badge-outline badge-secondary">
                        {tag.name}
                      </div>
                    </div>
                  ))}
                  <ExpandableTagsCell tags={record.tags} />
                </h2>
              </div>
            </div> */}
          </div>
        </div>
      ),
    },
    { label: "Timeseries Viewer", content: "" },
    { label: "Graph Viewer", content: "" },
    { label: "Record History", content: "" },
  ];

  if (!record)
    return <div className="loading loading-spinner loading-xl"></div>;

  return (
    <div>
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-info-content">
            {record?.name}
          </h1>
        </div>
      </div>

      <div className="divider"></div>

      <Tabs tabs={tabs} className={""}></Tabs>
    </div>
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

const RecordViewPage = () => {
  return (
    <Suspense
      fallback={<div className="loading loading-spinner loading-lg"></div>}
    >
      <RecordViewPageContent />
    </Suspense>
  );
};

export default RecordViewPage;
