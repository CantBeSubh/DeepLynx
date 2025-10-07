"use client";

import React, { useCallback, useState, useEffect } from "react";
import Link from "next/link";
import toast from "react-hot-toast";
import {
  XMarkIcon,
  PencilIcon,
  CheckCircleIcon,
  XCircleIcon,
} from "@heroicons/react/24/outline";

// Components
import TagButton from "@/app/(home)/components/TagButton";
import PropertyTable from "../components/PropertyTable";
import Tabs from "@/app/(home)/components/Tabs";
import ConfirmationModal from "@/app/(home)/components/ConfirmationModal";
import RecordViewModal from "@/app/(home)/components/RecordViewModal";
import RelatedRecordsCard, {
  CardColumn,
} from "./components/RelatedRecordsCard";

// Services
import {
  updateRecord,
  unAttachTagFromRecord,
  getRecord,
} from "@/app/lib/record_services.client";
import { getTagsForProjects } from "@/app/lib/query_services.client";
import {
  deleteEdge,
  getEdge,
  getEdgesByRecord,
} from "@/app/lib/edge_services.client";

// Types & Context
import { FileViewerTableRow, TagResponseDto } from "@/app/(home)/types/types";
import { useLanguage } from "@/app/contexts/Language";
import RelatedRecordsCardSkeleton from "./skeletons/RelatedRecordsSkeleton";

// ============= TYPE DEFINITIONS =============
interface Props {
  projectId: number;
  recordId: number;
}

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

interface ModalState {
  isOpen: boolean;
  type: "tag" | "relatedRecord" | null;
  nameToRemove: string;
  recordNameToRemove?: string;
  idToRemove: string | null;
  originId: number | null;
  destinationId: number | null;
}

// ============= MAIN COMPONENT =============
export default function RecordViewClient({ projectId, recordId }: Props) {
  const { t } = useLanguage();

  // ============= STATE MANAGEMENT =============
  // Record & Tags State
  const [record, setRecord] = useState<FileViewerTableRow | null>(null);
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [selectedTags, setSelectedTags] = useState<TagResponseDto[]>([]);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [tagsToRemove, setTagsToRemove] = useState<string[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [isLoadingRelatedRecords, setIsLoadingRelatedRecords] = useState(false);

  // Related Records State
  const [parsedRelatedRecords, setParsedRelatedRecords] = useState<
    ParsedRecord[]
  >([]);

  // Modal State
  const [modal, setModal] = useState<ModalState>({
    isOpen: false,
    type: null,
    nameToRemove: "",
    recordNameToRemove: "",
    idToRemove: null,
    originId: null,
    destinationId: null,
  });

  // UI State
  const [activeTab, setActiveTab] = useState(0);
  const [selectedRecord, setSelectedRecord] =
    useState<FileViewerTableRow | null>(null);
  const [isRecordViewModalOpen, setRecordViewModalOpen] = useState(false);

  // ============= EFFECTS =============
  // Reset state when recordId changes
  useEffect(() => {
    setRecord(null);
    setSelectedTags([]);
    setSelectedIds([]);
    setIsEditing(false);
    setTagsToRemove([]);
    setParsedRelatedRecords([]);
  }, [recordId]);

  // Fetch main record data
  useEffect(() => {
    const fetchRecord = async () => {
      if (!recordId || !projectId) return;

      try {
        const data = await getRecord(projectId, recordId);
        setRecord(data);

        if (data.tags) {
          const parsedTags = JSON.parse(data.tags);
          setSelectedTags(parsedTags);
          setSelectedIds(parsedTags.map((tag: { id: string }) => tag.id));
        }
      } catch (error) {
        console.error("Error fetching record:", error);
        toast.error("Failed to fetch record");
      }
    };

    fetchRecord();
  }, [recordId, projectId]);

  // Fetch available tags
  useEffect(() => {
    const fetchTags = async () => {
      if (!projectId) return;

      try {
        const data = await getTagsForProjects([projectId.toString()]);
        setTags(data);
      } catch (error) {
        console.error("Error fetching tags:", error);
      }
    };

    fetchTags();
  }, [projectId]);

  // Fetch related records
  useEffect(() => {
    const fetchRelatedRecords = async () => {
      if (!recordId || !projectId || !record) return;

      try {
        setIsLoadingRelatedRecords(true);
        setParsedRelatedRecords([]);
        const edges = await getEdgesByRecord(
          projectId.toString(),
          recordId,
          true
        );

        const promises = edges.map(async (edge: Edge) => {
          try {
            const isOrigin = edge.originId === recordId;
            const relatedRecordId = isOrigin
              ? edge.destinationId
              : edge.originId;
            const relatedRecord = await getRecord(projectId, relatedRecordId);

            return {
              relationship: `Relationship ${edge.relationshipId}`,
              id: relatedRecordId.toString(),
              class: relatedRecord.className || "Record",
              name: relatedRecord.name || `Record ${relatedRecordId}`,
              recordId: relatedRecordId,
              actions: (
                <XMarkIcon
                  className="w-5 h-5 cursor-pointer text-error hover:text-error-content"
                  onClick={() =>
                    handleOpenModal(
                      edge.id.toString(),
                      `Edge ${edge.id}`,
                      record?.name,
                      "relatedRecord",
                      edge.originId,
                      edge.destinationId
                    )
                  }
                />
              ),
            };
          } catch (error) {
            console.error(`Error fetching edge ${edge.id}:`, error);
            return null;
          }
        });

        const results = await Promise.all(promises);
        setParsedRelatedRecords(
          results.filter((r): r is ParsedRecord => r !== null)
        );
        setIsLoadingRelatedRecords(false);
      } catch (error) {
        console.error("Error fetching related records:", error);
        toast.error("Failed to fetch related records");
      }
    };

    fetchRelatedRecords();
  }, [recordId, projectId, record]);

  // ============= HANDLERS =============
  const handleOpenModal = useCallback(
    (
      id: string,
      name: string,
      recordName: string | undefined,
      type: "tag" | "relatedRecord",
      originId?: number,
      destinationId?: number
    ) => {
      setModal({
        isOpen: true,
        type,
        nameToRemove: name,
        recordNameToRemove: recordName,
        idToRemove: id,
        originId: originId || Number(id),
        destinationId: destinationId || Number(record?.id),
      });
    },
    [record?.id]
  );

  const handleCloseModal = () => {
    setModal((prev) => ({ ...prev, isOpen: false }));
  };

  const handleTagSelectionChange = async (selected: string[]) => {
    if (JSON.stringify(selected) === JSON.stringify(selectedIds)) return;

    const newTags = tags.filter((tag) => selected.includes(tag.id.toString()));
    setSelectedTags(newTags);
    setSelectedIds(selected);
    setRecord((prev) =>
      prev ? { ...prev, tags: JSON.stringify(newTags) } : prev
    );
  };

  const toggleEditMode = () => {
    setIsEditing(!isEditing);
    if (isEditing) setTagsToRemove([]);
  };

  const handleSaveTagChanges = async () => {
    try {
      for (const tagId of tagsToRemove) {
        await unAttachTagFromRecord(projectId, recordId, Number(tagId));
      }

      setSelectedTags((prev) =>
        prev.filter((tag) => !tagsToRemove.includes(tag.id.toString()))
      );
      setSelectedIds((prev) => prev.filter((id) => !tagsToRemove.includes(id)));
      setTagsToRemove([]);
      setIsEditing(false);
      toast.success("Tags updated successfully");
    } catch (error) {
      console.error("Error saving tags:", error);
      toast.error("Failed to update tags");
    }
  };

  const handleConfirmUnlink = async () => {
    const { type, idToRemove, nameToRemove, originId, destinationId } = modal;

    if (type === "tag" && idToRemove) {
      setTagsToRemove((prev) =>
        prev.includes(idToRemove)
          ? prev.filter((id) => id !== idToRemove)
          : [...prev, idToRemove]
      );
      toast.success(`Tag ${nameToRemove} marked for removal`);
    } else if (type === "relatedRecord" && originId && destinationId) {
      try {
        const edgeExists = await getEdge(
          projectId,
          null,
          String(originId),
          String(destinationId)
        );

        if (edgeExists) {
          await deleteEdge(
            projectId,
            null,
            String(originId),
            String(destinationId)
          );
          setParsedRelatedRecords((prev) =>
            prev.filter((r) => r.id !== idToRemove)
          );
          toast.success("Link removed successfully");
        }
      } catch (error) {
        console.error("Error removing link:", error);
        toast.error("Failed to remove link");
      }
    }

    handleCloseModal();
  };

  const handleUpdateRecord = async (
    field: string,
    value: string,
    successMessage: string
  ) => {
    try {
      const update = await updateRecord(projectId, recordId, {
        [field]: value,
      });
      setRecord(update);
      toast.success(successMessage);
    } catch (error) {
      toast.error(`Failed to update ${field}`);
    }
  };

  // ============= RENDER HELPERS =============
  if (!record) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="loading loading-spinner loading-xl" />
      </div>
    );
  }

  const systemPropertiesRows = [
    {
      label: "Record Name",
      value: record.name,
      editable: true,
      onEdit: (value: string) =>
        handleUpdateRecord("name", value, "Record name updated"),
    },
    {
      label: "Record Description",
      value: record.description,
      editable: true,
      onEdit: (value: string) =>
        handleUpdateRecord("description", value, "Description updated"),
    },
    { label: "Data Source Name", value: record.dataSourceName },
    { label: "Uri", value: record.uri },
    {
      label: "Class Name",
      value: record.className,
      onEdit: (value: string) =>
        handleUpdateRecord("class_name", value, "Class name updated"),
    },
    { label: "Original ID", value: record.originalId },
    { label: "Last Updated At", value: record.lastUpdatedAt },
  ];

  const parsedProperties = record.properties
    ? JSON.parse(record.properties)
    : {};
  const additionalPropertiesRows = Object.entries(parsedProperties).map(
    ([key, value]) => ({
      label: key,
      value: typeof value === "object" ? JSON.stringify(value) : String(value),
    })
  );

  const relatedRecordsColumns: CardColumn<ParsedRecord>[] = [
    { key: "id", label: "Record ID" },
    { key: "class", label: "Relationship" },
    {
      key: "name",
      label: "Record Name",
      render: (row: ParsedRecord) => (
        <Link
          href={`/record?recordId=${row.recordId}&projectId=${projectId}`}
          className="text-primary hover:text-primary-content hover:underline"
        >
          {row.name}
        </Link>
      ),
    },
    { key: "actions", label: "Actions" },
  ];

  const tabs = [
    {
      label: "Record Information",
      content: (
        <div className="flex gap-6">
          {/* Left Column - Properties */}
          <div className="w-full md:w-1/2 space-y-4">
            <PropertyTable
              title="System Properties"
              rows={systemPropertiesRows}
              download={
                !!record.uri &&
                record.uri.trim().length > 0 &&
                record.uri.toLowerCase() !== "null"
              }
              recordName={record.name}
            />
            <PropertyTable
              title="Additional Properties"
              rows={additionalPropertiesRows}
            />
          </div>

          {/* Right Column - Tags & Relations */}
          <div className="flex-1 space-y-4">
            {/* Tags Card */}
            <div className="card bg-base-100 shadow-md p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-bold text-base-content">
                  {t.translations.TAGS}
                </h2>
                <div className="flex items-center gap-2">
                  <TagButton
                    tags={tags}
                    onSelectionChange={handleTagSelectionChange}
                    projectId={projectId}
                    recordId={recordId}
                    selectedIds={selectedIds}
                    setSelectedIds={setSelectedIds}
                  />
                  {!isEditing ? (
                    <PencilIcon
                      className="w-6 h-6 cursor-pointer text-primary hover:text-primary-content"
                      onClick={toggleEditMode}
                    />
                  ) : (
                    <>
                      <CheckCircleIcon
                        className="w-6 h-6 cursor-pointer text-success hover:text-success-content"
                        onClick={handleSaveTagChanges}
                      />
                      <XCircleIcon
                        className="w-6 h-6 cursor-pointer text-error hover:text-error-content"
                        onClick={toggleEditMode}
                      />
                    </>
                  )}
                </div>
              </div>

              <div className="flex flex-wrap gap-2">
                {selectedTags.map((tag) => (
                  <span
                    key={tag.id}
                    className="flex items-center border-2 border-primary text-primary rounded-full px-3 py-1 hover:bg-primary hover:text-primary-content transition-colors"
                  >
                    {tag.name}
                    {isEditing && !tagsToRemove.includes(tag.id.toString()) && (
                      <XMarkIcon
                        className="w-4 h-4 ml-1 cursor-pointer text-error hover:text-error-content"
                        onClick={() =>
                          handleOpenModal(
                            tag.id.toString(),
                            tag.name,
                            record.name,
                            "tag"
                          )
                        }
                      />
                    )}
                  </span>
                ))}
              </div>
            </div>

            {/* Related Records Card */}
            {isLoadingRelatedRecords ? (
              <RelatedRecordsCardSkeleton rows={4} columns={2} />
            ) : (
              <RelatedRecordsCard
                columns={relatedRecordsColumns}
                rows={parsedRelatedRecords}
              />
            )}
          </div>
        </div>
      ),
    },
  ];

  // ============= MAIN RENDER =============
  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-4">
        <h1 className="text-2xl font-bold text-base-content">{record.name}</h1>
      </div>

      <Tabs
        tabs={tabs}
        className="ml-6 pt-6"
        activeTab={tabs[activeTab].label}
        onTabChange={(label) =>
          setActiveTab(tabs.findIndex((tab) => tab.label === label))
        }
      />

      <ConfirmationModal
        isOpen={modal.isOpen}
        onClose={handleCloseModal}
        onConfirm={handleConfirmUnlink}
        tagName={modal.nameToRemove}
        recordName={modal.recordNameToRemove}
      />

      <RecordViewModal
        isOpen={isRecordViewModalOpen}
        onClose={() => setRecordViewModalOpen(false)}
        record={selectedRecord}
        relatedRecords={parsedRelatedRecords}
        tags={tags}
      />
    </div>
  );
}
