"use client";
import React, { useCallback, useState, useEffect, useMemo } from "react";
import Link from "next/link";
import toast from "react-hot-toast";
import {
  updateRecord,
  unAttachTagFromRecord,
  getRecord,
} from "@/app/lib/record_services.client";
import { getTagsForProjects } from "@/app/lib/query_services.client";
import PropertyTable from "../components/PropertyTable";
import Tabs from "@/app/(home)/components/Tabs";
import { TagResponseDto } from "../types/responseDTOs";
import {
  XMarkIcon,
  PencilIcon,
  CheckCircleIcon,
  XCircleIcon,
} from "@heroicons/react/24/outline";
import RecordLoading from "./loading";

// Components
import TagButton from "@/app/(home)/components/TagButton";
import ConfirmationModal from "@/app/(home)/components/ConfirmationModal";
import RelatedRecordsCard, {
  CardColumn,
} from "./components/RelatedRecordsCard";

// Services
import {
  archiveEdge,
  getEdge,
  getEdgesByRecord,
} from "@/app/lib/edge_services.client";

// Types & Context
import { FileViewerTableRow } from "@/app/(home)/types/types";
import { useLanguage } from "@/app/contexts/Language";
import RelatedRecordsCardSkeleton from "./skeletons/RelatedRecordsSkeleton";
import { RelatedRecordsResponseDto } from "../types/responseDTOs";
import { RelatedRecordsRequestDto } from "../types/requestDTOs";
import GraphClientPage from "../graph/GraphClientPage";

// ============= TYPE DEFINITIONS =============
interface Props {
  projectId: number;
  recordId: number;
}

interface RelatedRecordViewModel extends RelatedRecordsResponseDto {
  actions: React.JSX.Element;
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

  // Pagination State
  const [originPage, setOriginPage] = useState(1);
  const [destinationPage, setDestinationPage] = useState(1);
  const [pageSize] = useState(20);
  const [hasMoreOrigins, setHasMoreOrigins] = useState(true);
  const [hasMoreDestinations, setHasMoreDestinations] = useState(true);

  // Related Records State
  const [originRecords, setOriginRecords] = useState<RelatedRecordViewModel[]>(
    []
  );
  const [destinationRecords, setDestinationRecords] = useState<
    RelatedRecordViewModel[]
  >([]);
  const [isLoadingOrigins, setIsLoadingOrigins] = useState(false);
  const [isLoadingDestinations, setIsLoadingDestinations] = useState(false);

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

  // ============= HANDLERS =============
  const handleUpdateRecord = useCallback(
    async (field: string, value: string, successMessage: string) => {
      try {
        const update = await updateRecord(projectId, recordId, {
          [field]: value,
        });
        setRecord((prev) => (prev ? { ...prev, ...update } : update));
        toast.success(successMessage);
      } catch (error) {
        toast.error(`${t.translations.FAILED_TO_UPDATE} ${field}`);
      }
    },
    [projectId, recordId, t.translations.FAILED_TO_UPDATE]
  );

  const resetAllState = useCallback(() => {
    setRecord(null);
    setSelectedTags([]);
    setSelectedIds([]);
    setIsEditing(false);
    setTagsToRemove([]);
    setOriginPage(1);
    setDestinationPage(1);
    setOriginRecords([]);
    setDestinationRecords([]);
    setHasMoreOrigins(true);
    setHasMoreDestinations(true);
  }, []);

  // Create a reusable fetch function
  const fetchRelatedRecords = useCallback(
    async (
      isOrigin: boolean,
      page: number,
      setLoading: (val: boolean) => void,
      setHasMore: (val: boolean) => void,
      setRecords: React.Dispatch<React.SetStateAction<RelatedRecordViewModel[]>>
    ) => {
      if (!recordId || !projectId || !record) return;

      try {
        setLoading(true);

        const params: RelatedRecordsRequestDto = {
          recordId: recordId,
          isOrigin: isOrigin,
          page: page,
          pageSize: pageSize,
          hideArchived: true,
        };

        const edges: RelatedRecordsResponseDto[] = await getEdgesByRecord(
          projectId.toString(),
          params
        );

        if (!edges || edges.length === 0) {
          setHasMore(false);
          if (page === 1) {
            setRecords([]);
          }
          setLoading(false);
          return;
        }

        if (edges.length < pageSize) {
          setHasMore(false);
        }

        const viewModels: RelatedRecordViewModel[] = edges
          .filter(
            (edge) => edge.relatedRecordId != null && edge.relatedRecordId > 0
          )
          .map((edge) => ({
            relatedRecordName: edge.relatedRecordName,
            relatedRecordId: edge.relatedRecordId,
            relatedRecordProjectId: edge.relatedRecordProjectId,
            relationshipName: edge.relationshipName,
            actions: (
              <XMarkIcon
                className="w-5 h-5 cursor-pointer text-error hover:text-error-content"
                onClick={() => {
                  setModal({
                    isOpen: true,
                    type: "relatedRecord",
                    nameToRemove: edge.relationshipName || t.translations.EDGE,
                    recordNameToRemove: record?.name,
                    idToRemove: edge.relatedRecordId!.toString(),
                    originId: isOrigin ? recordId : edge.relatedRecordId!,
                    destinationId: isOrigin ? edge.relatedRecordId! : recordId,
                  });
                }}
              />
            ),
          }));

        if (page === 1) {
          setRecords(viewModels);
        } else {
          setRecords((prev) => [...prev, ...viewModels]);
        }

        setLoading(false);
      } catch (error) {
        console.error(
          `Error fetching ${isOrigin ? "origin" : "destination"} records:`,
          error
        );
        setLoading(false);
      }
    },
    [recordId, projectId, record, pageSize, t.translations.EDGE]
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
      toast.success(t.translations.TAGS_UPDATED_SUCCESS);
    } catch (error) {
      console.error("Error saving tags:", error);
      toast.error(t.translations.FAILED_TO_UPDATE_TAGS);
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
      toast.success(
        `${t.translations.TAG_} ${nameToRemove} ${t.translations._MARKED_FOR_REMOVAL}`
      );
    } else if (type === "relatedRecord" && originId && destinationId) {
      try {
        const edgeExists = await getEdge(
          projectId,
          null,
          String(originId),
          String(destinationId)
        );

        if (edgeExists) {
          await archiveEdge(projectId, null, originId, destinationId);

          if (originId === recordId) {
            setOriginRecords((prev) =>
              prev.filter((r) => r.relatedRecordId !== Number(idToRemove))
            );
          } else {
            setDestinationRecords((prev) =>
              prev.filter((r) => r.relatedRecordId !== Number(idToRemove))
            );
          }

          toast.success(t.translations.LINK_ARCHIVED_SUCCESS);
        }
      } catch (error) {
        console.error("Error archiving link:", error);
        toast.error(t.translations.FAILED_TO_ARCHIVE_LINK);
      }
    }

    handleCloseModal();
  };

  const handleOpenModal = useCallback(
    (
      id: string,
      name: string,
      recordName: string | undefined,
      type: "tag" | "relatedRecord"
    ) => {
      setModal({
        isOpen: true,
        type,
        nameToRemove: name,
        recordNameToRemove: recordName,
        idToRemove: id,
        originId: null,
        destinationId: null,
      });
    },
    []
  );

  // ============= EFFECTS =============
  useEffect(() => {
    resetAllState();
  }, [recordId, resetAllState]);

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
        toast.error(t.translations.FAILED_TO_FETCH_RECORD);
      }
    };

    fetchRecord();
  }, [recordId, projectId, t.translations.FAILED_TO_FETCH_RECORD]);

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

  // Fetch origin records
  useEffect(() => {
    fetchRelatedRecords(
      true,
      originPage,
      setIsLoadingOrigins,
      setHasMoreOrigins,
      setOriginRecords
    );
  }, [fetchRelatedRecords, originPage]);

  // Fetch destination records
  useEffect(() => {
    fetchRelatedRecords(
      false,
      destinationPage,
      setIsLoadingDestinations,
      setHasMoreDestinations,
      setDestinationRecords
    );
  }, [fetchRelatedRecords, destinationPage]);

  // ============= MEMOIZED VALUES =============
  const systemPropertiesRows = useMemo(() => {
    if (!record) return [];
    return [
      {
        label: t.translations.RECORD_NAME,
        value: record.name,
        editable: true,
        onEdit: (value: string) =>
          handleUpdateRecord("name", value, t.translations.RECORD_NAME_UPDATED),
      },
      {
        label: t.translations.RECORD_DESCRIPTION,
        value: record.description,
        editable: true,
        onEdit: (value: string) =>
          handleUpdateRecord(
            "description",
            value,
            t.translations.RECORD_NAME_UPDATED
          ),
      },
      { label: t.translations.DATA_SOURCE_NAME, value: record.dataSourceName },
      { label: t.translations.URI, value: record.uri },
      {
        label: t.translations.CLASS_NAME,
        value: record.className,
        onEdit: (value: string) =>
          handleUpdateRecord(
            "class_name",
            value,
            t.translations.CLASS_NAME_UPDATED
          ),
      },
      { label: t.translations.ORIGINAL_ID, value: record.originalId },
      { label: t.translations.LAST_UPDATED_AT, value: record.lastUpdatedAt },
    ];
  }, [record, handleUpdateRecord, t.translations]);

  const additionalPropertiesRows = useMemo(() => {
    if (!record?.properties) return [];
    const parsedProperties = JSON.parse(record.properties);
    return Object.entries(parsedProperties).map(([key, value]) => ({
      label: key,
      value: typeof value === "object" ? JSON.stringify(value) : String(value),
    }));
  }, [record?.properties]);

  const relatedRecordsColumns: CardColumn<RelatedRecordViewModel>[] = [
    {
      key: "relationshipName",
      label: t.translations.RELATIONSHIP,
      render: (row) => <span>{row.relationshipName || "-"}</span>,
    },
    {
      key: "relatedRecordName",
      label: t.translations.RECORD_NAME,
      render: (row) =>
        row.relatedRecordId ? (
          <Link
            href={`/record?recordId=${row.relatedRecordId}&projectId=${projectId}`}
            className="text-primary hover:text-primary-content hover:underline"
          >
            {row.relatedRecordName ||
              `${t.translations.RECORD_} ${row.relatedRecordId}`}
          </Link>
        ) : (
          <span>{row.relatedRecordName || t.translations.UNKOWN}</span>
        ),
    },
    { key: "actions", label: t.translations.ACTIONS },
  ];

  // ============= RENDER HELPERS =============
  if (!record) {
    return <RecordLoading />;
  }

  const tabs = [
    {
      label: t.translations.RECORD_INFORMATION,
      content: (
        <div className="flex gap-6 mt-4">
          {/* Left Column - Properties */}
          <div className="w-full md:w-1/2 space-y-4">
            <PropertyTable
              title={t.translations.SYSTEM_PROPERTIES}
              rows={systemPropertiesRows}
              download={
                !!record.uri &&
                record.uri.trim().length > 0 &&
                record.uri.toLowerCase() !== "null"
              }
              recordName={record.name}
            />
            <PropertyTable
              title={t.translations.ADDITIONAL_PROPERIES}
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

            {/* Related Records Card - Origins */}
            {isLoadingOrigins && originPage === 1 ? (
              <RelatedRecordsCardSkeleton rows={6} columns={3} />
            ) : (
              <RelatedRecordsCard
                title={`${t.translations.OUTGOING}${record.name}${t.translations.OUTGOING_ARROW}`}
                columns={relatedRecordsColumns}
                rows={originRecords}
                onLoadMore={() => {
                  if (!isLoadingOrigins && hasMoreOrigins) {
                    setOriginPage((prev) => prev + 1);
                  }
                }}
                isLoading={isLoadingOrigins && originPage > 1}
                hasMore={hasMoreOrigins}
              />
            )}

            {/* Related Records Card - Destinations */}
            {isLoadingDestinations && destinationPage === 1 ? (
              <RelatedRecordsCardSkeleton rows={6} columns={3} />
            ) : (
              <div className="mt-4">
                <RelatedRecordsCard
                  title={`${t.translations.INCOMING}${record.name}${t.translations.INCOMING_ARROW}`}
                  columns={relatedRecordsColumns}
                  rows={destinationRecords}
                  onLoadMore={() => {
                    if (!isLoadingDestinations && hasMoreDestinations) {
                      setDestinationPage((prev) => prev + 1);
                    }
                  }}
                  isLoading={isLoadingDestinations && destinationPage > 1}
                  hasMore={hasMoreDestinations}
                />
              </div>
            )}
          </div>
        </div>
      ),
    },
    {
      label: "Graph",
      content: (
        <GraphClientPage projectId={String(projectId)} recordId={recordId} />
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
    </div>
  );
}
