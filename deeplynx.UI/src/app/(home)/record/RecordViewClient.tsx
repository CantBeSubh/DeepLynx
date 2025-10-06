// src/app/(home)/record/RecordViewClient.tsx

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
} from "@heroicons/react/24/outline";
import GenericTable from "@/app/(home)/components/GenericTable";
import ConfirmationModal from "@/app/(home)/components/ConfirmationModal";
import RecordViewModal from "@/app/(home)/components/RecordViewModal";
import RelatedRecordsCard, {
  CardColumn,
} from "./components/RelatedRecordsCard";
import { useLanguage } from "@/app/contexts/Language";
import {
  deleteEdge,
  getEdge,
  getEdgesByRecord,
} from "@/app/lib/edge_services.client";
import Link from "next/link";
import { ClientPageRoot } from "next/dist/client/components/client-page";

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
  recordId: number;
  actions: React.JSX.Element;
}

interface Edge {
  id: number;
  originId: number;
  destinationId: number;
  relationshipId: number;
  dataSourceId: number;
  projectId: number;
  lastUpdatedAt: string;
  lastUpdatedBy: string | null;
  isArchived: boolean;
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
  const { t } = useLanguage();
  const [record, setRecord] = useState<FileViewerTableRow | null>(
    initialRecord
  );
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [selectedTags, setSelectedTags] = useState<TagResponseDto[]>([]);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [tagsToRemove, setTagsToRemove] = useState<string[]>([]);
  const [activeTab, setActiveTab] = useState(0);
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

  useEffect(() => {
    const fetchDetailedRelatedRecords = async () => {
      if (!recordId || !projectId || hasFetchedRelatedRecords) return;

      try {
        // Fetch edges for this record
        const edges = await getEdgesByRecord(
          projectId.toString(),
          recordId,
          true
        );
        console.log("edges", edges)
        // For each edge, you might want to fetch the actual record details
        // This assumes you have a getRecord endpoint that can fetch by ID
        const parsedPromises = edges.map(async (edge: Edge) => {
          try {
            // Determine if current record is origin or destination
            const isOrigin = edge.originId === recordId;
            const relatedRecordId = isOrigin
              ? edge.destinationId
              : edge.originId;

            // Fetch the related record details
            const relatedRecord = await getRecord(projectId, relatedRecordId);

            return {
              relationship: `Relationship ${edge.relationshipId}`,
              id: relatedRecordId.toString(),
              class: relatedRecord.className || "Record",
              name: relatedRecord.name || `Record ${relatedRecordId}`,
              recordId: relatedRecordId,
              actions: (
                <div className="flex gap-2">
                  <XMarkIcon
                    className="w-5 h-5 cursor-pointer text-error hover:text-error-content"
                    onClick={() => {
                      setSelectedOriginId(edge.originId);
                      setSelectedDestinationId(edge.destinationId);
                      handleToggleToRemove(
                        edge.id.toString(),
                        `Edge ${edge.id}`,
                        record?.name,
                        "relatedRecord"
                      );
                    }}
                  />
                </div>
              ),
            };
          } catch (error) {
            console.error(`Error fetching details for edge ${edge.id}:`, error);
            return null;
          }
        });

        const results = await Promise.all(parsedPromises);
        const validResults = results.filter(
          (r): r is ParsedRecord => r !== null
        );

        setParsedRelatedRecords(validResults);
        setHasFetchedRelatedRecords(true);
      } catch (error) {
        console.error("Error fetching related records:", error);
        toast.error("Failed to fetch related records");
      }
    };

    fetchDetailedRelatedRecords();
  }, [
    recordId,
    projectId,
    hasFetchedRelatedRecords,
    record?.name,
    handleToggleToRemove,
  ]);

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
    { label: "Last Updated At", value: record.lastUpdatedAt },
  ];

  const relatedRecordsColumn: CardColumn<ParsedRecord>[] = [
    { key: "id", label: "ID" },
    { key: "class", label: "Class" },
    { 
      key: "name", 
      label: "Name",
      render: (row: ParsedRecord) => (
        <Link 
          href={`/record?recordId=${row.recordId}&projectId=${projectId}`}
          className="text-primary hover:text-primary-content hover:underline"
        >
          {row.name}
        </Link>
      )
    },
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
        <div className="flex">
          <div className="w-full md:w-1/2 p-3">
            <PropertyTable
              className=""
              title="System Properties"
              rows={systemPropertiesRows}
              download={
                !!record?.uri &&
                record.uri.trim().length > 0 &&
                record.uri.toLowerCase() !== "null"
              }
              recordName={record?.name}
            />
            <PropertyTable
              className="mt-4"
              title="Additional Properties"
              rows={additionalPropertiesRows}
            />
          </div>
          <div className="flex-1 mt-3 mr-6">
            <div className="card bg-base-100 shadow-md p-4 flex">
              <div className="flex items-center">
                <h2 className="text-xl font-bold mb-4 p-4 text-base-content">
                  {t.translations.TAGS}
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
  ];

  // Function to handle tab change
  const handleTabChange = (label: string) => {
    const index = tabs.findIndex((tab) => tab.label === label);
    if (index !== -1) {
      setActiveTab(index);
    }
  };

  return (
    <div>
      <div className="flex justify-between items-center bg-base-200/40 pl-12 p-2 pb-4">
        <div>
          <h1 className="text-2xl font-bold text-base-content p-2">
            {record?.name}
          </h1>
        </div>
      </div>

      <Tabs
        tabs={tabs}
        className={"ml-6 pt-6"}
        activeTab={tabs[activeTab].label}
        onTabChange={handleTabChange}
      />
    </div>
  );
}