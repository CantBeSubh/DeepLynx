// src/app/

"use client";

import { useCallback, useState } from "react";
import React, { useEffect } from "react";
import toast from "react-hot-toast";
import TagButton from "@/app/(home)/components/TagButton";
import {
  updateRecord,
  unAttachTagFromRecord,
  getRecord,
} from "@/app/lib/record_services.client";
import { getTagsForProjects } from "@/app/lib/query_services.client";
import PropertyTable from "../components/PropertyTable";
import Tabs from "@/app/(home)/components/Tabs";
import {
  Column,
  FileViewerTableRow,
  TagResponseDto,
} from "@/app/(home)/types/types";
import {
  XMarkIcon,
  PencilIcon,
  CheckCircleIcon,
  XCircleIcon,
  EyeIcon,
} from "@heroicons/react/24/outline";
import GenericTable from "@/app/(home)/components/GenericTable";
import { getNodesWithinDepth, queryKuzu } from "@/app/lib/kuzu_services";
import ConfirmationModal from "@/app/(home)/components/ConfirmationModal";
import RecordViewModal from "@/app/(home)/components/RecordViewModal";
import { deleteEdge, getEdge } from "@/app/lib/edge_services.client";
import RelatedRecordsCard, {
  CardColumn,
} from "./components/RelatedRecordsCard";

type Props = {
  initialRecord: FileViewerTableRow | null;
  projectId: number;
  recordId: number;
};

interface ParsedRecord {
  relationship: string;
  id: string;
  class: string;
  name: string;
  actions: React.JSX.Element;
}

type Record = {
  recordId: number;
  internalId: string;
  label: string;
  id: number;
  projectId: number;
  className: string;
  projectName: string;
  name: string;
  uri: string;
  dataSourceId: number;
  originalId: string;
  classId: number;
  properties: string;
  tags: string;
  createdBy: string;
  createdAt: string;
  modifiedAt: string;
  modifiedBy: string;
  archivedAt: string | null;
  lastUpdatedAt: string;
};

type Relationship = {
  relationshipName: string;
  sourceId: string;
  destId: string;
};

type RelatedRecord = Relationship | Record;

export default function RecordViewClient({
  initialRecord,
  projectId,
  recordId,
}: Props) {
  const [record, setRecord] = useState<FileViewerTableRow | null>(
    initialRecord
  );
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [selectedTags, setSelectedTags] = useState<TagResponseDto[]>([]);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [tagsToRemove, setTagsToRemove] = useState<string[]>([]);
  const [relatedRecords, setRelatedRecords] = useState<RelatedRecord[]>();
  const [hasFetchedRelatedRecords, setHasFetchedRelatedRecords] =
    useState(false);
  const [parsedRelatedRecords, setParsedRelatedRecords] = useState<
    ParsedRecord[]
  >([]);
  const [isModalOpen, setModalOpen] = useState(false);
  const [selectedNameToRemove, setSelectedNameToRemove] = useState<string>("");
  const [selectedRecordNameToRemove, setSelectedRecordNameToRemove] = useState<
    string | undefined
  >("");
  const [selectedRecord, setSelectedRecord] =
    useState<FileViewerTableRow | null>(null);
  const [idToRemove, setIdToRemove] = useState<string | null>(null);
  const [isRecordViewModalOpen, setRecordViewModalOpen] = useState(false);
  const [selectedOriginId, setSelectedOriginId] = useState<number | null>(null);
  const [selectedDestinationId, setSelectedDestinationId] = useState<
    number | null
  >(null);
  const [confirmationType, setConfirmationType] = useState<
    "tag" | "relatedRecord" | null
  >(null);
  const [relationship, setRelationship] = useState<string | null>(null);

  function formatDate(date?: string | null) {
    if (!date) return "N/A";
    return new Date(date).toLocaleDateString("en-US", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
  }

  const handleToggleToRemove = useCallback(
    (
      id: string,
      name: string,
      recordName: string | undefined,
      type: "tag" | "relatedRecord"
    ) => {
      setSelectedNameToRemove(name);
      setSelectedRecordNameToRemove(recordName);
      setModalOpen(true);
      setIdToRemove(id);
      setSelectedOriginId(Number(id));
      setSelectedDestinationId(Number(record?.id));
      setConfirmationType(type);
    },
    [record?.id]
  );

  useEffect(() => {
    const fetchData = async () => {
      try {
        if (recordId && projectId) {
          const data = await getRecord(Number(projectId), Number(recordId));
          setRecord(data);
          if (data.tags) {
            const tags = JSON.parse(data.tags);
            setSelectedTags(tags);
            setSelectedIds(tags.map((tag: { id: string }) => tag.id));
          }
        }
      } catch (error) {
        console.error("Error fetching record:", error);
      }
    };
    fetchData();
  }, [recordId, projectId]);

  useEffect(() => {
    const fetchRelatedRecords = async () => {
      try {
        if (record && !hasFetchedRelatedRecords) {
          const request = {
            tablename: record.className,
            id: record.id,
            depth: 1,
          };

          const relatedRecordsData = await getNodesWithinDepth(
            Number(projectId),
            request
          );

          setRelatedRecords(relatedRecordsData);

          setHasFetchedRelatedRecords(true);
        }
      } catch (error) {
        console.error("Error fetching related records:", error);
      }
    };
    if (record?.className && record?.id) {
      if (record?.className && record?.id) {
        fetchRelatedRecords();
      }
    }
  }, [initialRecord, hasFetchedRelatedRecords, projectId, record]);

  useEffect(() => {
    const parseRelatedRecords = (
      relatedRecords: RelatedRecord[] | undefined
    ) => {
      if (!relatedRecords) return [];

      const relationshipNames: string[] = [];
      const classNames: string[] = [];
      const names: string[] = [];
      const recordIds: number[] = [];

      relatedRecords.forEach((item) => {
        if ("relationshipName" in item) {
          relationshipNames.push(item.relationshipName);
        } else {
          if (item.recordId == recordId) {
            return;
          }
          classNames.push(item.className);
          names.push(item.name);
          recordIds.push(item.recordId);
        }
      });

      const relatedRecordsArray: ParsedRecord[] = [];
      const relationshipIndex = 0;

      relatedRecords.forEach((item, _) => {
        if (!("relationshipName" in item)) {
          if (item.recordId == recordId) {
            return;
          }
          const relationship =
            relationshipNames.length > relationshipIndex
              ? relationshipNames[relationshipIndex]
              : "";
          relatedRecordsArray.push({
            relationship: relationship,
            id: item.recordId.toString(),
            class: item.className,
            name: item.name,
            actions: (
              <div className="flex items-center">
                <button
                  className="text-blue-500 cursor-pointer"
                  onClick={async () => {
                    const selectedRecord = await getRecord(
                      Number(projectId),
                      item.recordId
                    );
                    setSelectedRecord(selectedRecord);
                    setRecordViewModalOpen(true);
                  }}
                >
                  <EyeIcon className="w-4 h-4" />
                </button>
                <button
                  className="text-red-500 ml-2 cursor-pointer border rounded px-1"
                  onClick={() => {
                    handleToggleToRemove(
                      item.recordId.toString(),
                      item.name,
                      item.className,
                      "relatedRecord"
                    );
                  }}
                >
                  Remove Link
                </button>
              </div>
            ),
          });
        }
      });

      return relatedRecordsArray;
      return relatedRecordsArray;
    };

    const parsedRecords = parseRelatedRecords(relatedRecords);
    setParsedRelatedRecords(parsedRecords);
  }, [relatedRecords, projectId, recordId, handleToggleToRemove]);

  useEffect(() => {
    const fetchTags = async () => {
      try {
        const data = await getTagsForProjects([projectId.toString()]);
        setTags(data);
      } catch (error) {
        console.error("Error fetching tags:", error);
      }
    };
    fetchTags();
  }, [projectId]);

  const handleTagSelectionChange = async (selected: string[]) => {
    if (JSON.stringify(selected) !== JSON.stringify(selectedIds)) {
      const newTags = tags.filter((tag) =>
        selected.includes(tag.id.toString())
      );
      setSelectedTags(newTags);
      setSelectedIds(selected);

      setRecord((prevRecord) => {
        if (!prevRecord) return prevRecord;
        const updatedTags = JSON.stringify(newTags);
        return { ...prevRecord, tags: updatedTags };
      });
    }
  };

  const toggleEditMode = () => {
    setIsEditing(!isEditing);
    if (isEditing) {
      setTagsToRemove([]);
    }
  };

  const handleSave = async () => {
    for (const tagId of tagsToRemove) {
      await unAttachTagFromRecord(
        Number(projectId),
        Number(recordId),
        Number(tagId)
      );
    }
    setSelectedTags((prevTags) =>
      prevTags.filter((tag) => !tagsToRemove.includes(tag.id.toString()))
    );
    setSelectedIds((prevIds) =>
      prevIds.filter((id) => !tagsToRemove.includes(id))
    );
    setTagsToRemove([]);
    setIsEditing(false);
  };

  const handleConfirmUnlink = async () => {
    if (confirmationType === "tag") {
      if (idToRemove) {
        setTagsToRemove((prev) =>
          prev.includes(idToRemove)
            ? prev.filter((id) => id !== idToRemove)
            : [...prev, idToRemove]
        );
        toast.success(`Tag ${selectedNameToRemove} removed successfully.`);
      }
    } else if (confirmationType === "relatedRecord") {
      if (idToRemove && selectedOriginId && selectedDestinationId) {
        try {
          if (
            await getEdge(
              projectId,
              null,
              String(selectedOriginId),
              String(selectedDestinationId)
            )
          ) {
            await deleteEdge(
              projectId,
              null,
              String(selectedOriginId),
              String(selectedDestinationId)
            );
          }
          await queryKuzu(
            projectId,
            `MATCH (m)-[r:${relationship}]->(n) WHERE m.record_id = ${selectedOriginId} AND n.record_id = ${selectedDestinationId} DELETE r;`
          );
          setParsedRelatedRecords((prevRecords) =>
            prevRecords.filter((record) => record.id !== idToRemove)
          );
          toast.success("Link removed successfully");
        } catch (error) {
          toast.error("Failed to remove link");
          console.error("Error removing link:", error);
        }
      }
    }
    setModalOpen(false);
  };

  if (!record) {
    return <div className="loading loading-spinner loading-xl" />;
  }

  const systemPropertiesRows = [
    {
      label: "Record Name",
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
    {
      label: "Data Source Name",
      value: record.dataSourceName,
    },
    { label: "Uri", value: record.uri },
    {
      label: "Class Name",
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
    { label: "Original ID", value: record.originalId },
    { label: "Created By", value: record.createdBy },
    { label: "Created At", value: formatDate(record.createdAt) },
    { label: "Modified By", value: record.modifiedBy },
    { label: "Modified At", value: formatDate(record.modifiedAt) },
  ];

  const relatedRecordsColumn: CardColumn<ParsedRecord>[] = [
    { key: "relationship", label: "Relationship" },
    { key: "id", label: "ID" },
    { key: "class", label: "Class" },
    { key: "name", label: "Name" },
    { key: "actions", label: "Actions" },
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

  const tabs = [
    {
      label: "Record Information",
      content: (
        <div className="flex gap-4 mt-6">
          <div className="w-full md:w-1/2 p-3">
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
            <div className="card bg-base-200 shadow-md p-4 relative mb-20">
              <div className="flex justify-between items-center">
                <h2 className="text-xl font-bold mb-4 p-4 text-base-content">
                  Tags:
                </h2>
                <div className="flex items-center">
                  <TagButton
                    tags={tags}
                    onSelectionChange={handleTagSelectionChange}
                    projectId={Number(projectId)}
                    recordId={Number(recordId)}
                    selectedIds={selectedIds}
                    setSelectedIds={setSelectedIds}
                  />
                  {!isEditing && (
                    <PencilIcon
                      className="w-6 h-6 ml-2 cursor-pointer text-primary hover:text-primary-content"
                      onClick={toggleEditMode}
                    />
                  )}
                  {isEditing && (
                    <>
                      <CheckCircleIcon
                        className="w-6 h-6 ml-2 cursor-pointer text-success hover:text-success-content"
                        onClick={handleSave}
                      />
                      <XCircleIcon
                        className="w-6 h-6 ml-2 cursor-pointer text-error hover:text-error-content"
                        onClick={toggleEditMode}
                      />
                    </>
                  )}
                </div>
              </div>
              <span className="flex items-center flex-wrap mt-2">
                {selectedTags.map((tag) => (
                  <span
                    key={tag.id}
                    className="font-inter flex items-center border-2 border-primary text-primary rounded-full px-2 py-1 mr-2 mb-1 flex-shrink-0 hover:bg-primary hover:text-primary-content transition-colors"
                  >
                    {tag.name}
                    {!tagsToRemove.includes(tag.id.toString()) && isEditing && (
                      <XMarkIcon
                        className="w-4 h-4 ml-1 cursor-pointer text-error hover:text-error-content"
                        onClick={() =>
                          handleToggleToRemove(
                            tag.id.toString(),
                            tag.name,
                            record?.name || "Unknown Record",
                            "tag"
                          )
                        }
                      />
                    )}
                  </span>
                ))}
              </span>
            </div>
            <RelatedRecordsCard
              columns={relatedRecordsColumn}
              rows={parsedRelatedRecords}
            />
          </div>
          <ConfirmationModal
            isOpen={isModalOpen}
            onClose={() => setModalOpen(false)}
            onConfirm={handleConfirmUnlink}
            tagName={selectedNameToRemove}
            recordName={selectedRecordNameToRemove}
          />
          <RecordViewModal
            isOpen={isRecordViewModalOpen}
            onClose={() => setRecordViewModalOpen(false)}
            record={selectedRecord}
            relatedRecords={parsedRelatedRecords}
            tags={tags}
          />
        </div>
      ),
    },
    { label: "Timeseries Viewer", content: "" },
    { label: "Graph Viewer", content: "" },
    { label: "Record History", content: "" },
  ];

  return (
    <div className="p-6">
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2 pb-4">
        <div>
          <h1 className="text-2xl font-bold text-base-content">
            {record?.name}
          </h1>
        </div>
      </div>

      <div className="divider"></div>

      <Tabs tabs={tabs} className={""}></Tabs>
    </div>
  );
}
