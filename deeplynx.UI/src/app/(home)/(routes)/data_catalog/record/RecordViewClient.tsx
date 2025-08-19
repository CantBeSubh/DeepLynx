"use client";

import { useState } from "react";
import React, { useEffect } from "react";
import toast from "react-hot-toast";
import TagButton from "@/app/(home)/components/TagButton"
import { updateRecord, unAttachTagFromRecord, getRecord } from "@/app/lib/record_services.client";
import { getAllTags } from "@/app/lib/tag_services";
import PropertyTable from "./PropertyTable";
import Tabs from "@/app/(home)/components/Tabs";
import { Column, FileViewerTableRow } from "@/app/(home)/types/types";
import { XMarkIcon, PencilIcon, CheckCircleIcon, XCircleIcon, EyeIcon } from "@heroicons/react/24/outline";
import GenericTable from "@/app/(home)/components/GenericTable";
import { getNodesWithinDepth, queryKuzu } from "@/app/lib/kuzu_services";
import ConfirmationModal from "@/app/(home)/components/ConfirmationModal";
import RecordViewModal from "@/app/(home)/components/RecordViewModal";
import { deleteEdge, getEdge } from "@/app/lib/edge_services";

type Props = {
  initialRecord: FileViewerTableRow | null;
  projectId: number;
  recordId: number;
};

interface RelatedRecord {
  relationship: string;
  id: string;
  class: string;
  name: string;
  actions: React.JSX.Element;
}

export default function RecordViewClient({
  initialRecord,
  projectId,
  recordId,
}: Props) {
  const [record, setRecord] = useState<FileViewerTableRow>(initialRecord);
  const [tags, setTags] = useState<{ id: string; name: string }[]>([]);
  const [selectedTags, setSelectedTags] = useState<{ id: string; name: string }[]>([]);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [tagsToRemove, setTagsToRemove] = useState<string[]>([]);
  const [relatedRecords, setRelatedRecords] = useState<string>();
  const [hasFetchedRelatedRecords, setHasFetchedRelatedRecords] = useState(false);
  const [parsedRelatedRecords, setParsedRelatedRecords] = useState<RelatedRecord[]>([]);
  const [isModalOpen, setModalOpen] = useState(false);
  const [selectedNameToRemove, setSelectedNameToRemove] = useState<string>('');
  const [selectedRecordNameToRemove, setSelectedRecordNameToRemove] = useState<string | undefined>('');
  const [selectedRecord, setSelectedRecord] = useState<FileViewerTableRow | null>(null);
  const [idToRemove, setIdToRemove] = useState<string | null>(null);
  const [isRecordViewModalOpen, setRecordViewModalOpen] = useState(false);
  const [selectedOriginId, setSelectedOriginId] = useState<number | null>(null);
  const [selectedDestinationId, setSelectedDestinationId] = useState<number | null>(null);
  const [confirmationType, setConfirmationType] = useState<'tag' | 'relatedRecord' | null>(null);
  const [relationship, setRelationship] = useState<string | null>(null);

  function formatDate(date?: string | null) {
    if (!date) return "N/A";
    return new Date(date).toLocaleDateString("en-US", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
  }

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
            depth: 1
          }

          const relatedRecordsData = await getNodesWithinDepth(Number(projectId), request)

          setRelatedRecords(relatedRecordsData)

          setHasFetchedRelatedRecords(true);

          console.log("Has Fetched Set True")

        }
        
      } catch (error) {
        console.error("Error fetching related records:", error)
      }
    };
    if (record?.className && record?.id){
      fetchRelatedRecords();
    }
  }, [initialRecord])

  useEffect(() => {
    console.log(relatedRecords);

    const parseRelatedRecords = (relatedRecordsString: string | undefined) => {
      if (!relatedRecordsString) return [];

      const relatedRecordsArray: RelatedRecord[] = [];
      const mainRegex = /Relationship:\s*\((.*?)\)-\{_LABEL:\s*(\w+)\}\->[\s\S]*?Related_Node:[\s\S]*?class_name:\s*(\w+)[\s\S]*?properties:\s*{[^}]*?"name":\s*"(.*?)"/g;
      const recordIdRegex = /Related_Node:[\s\S]*?record_id:\s*(\d+)/g;
      const mainMatches: RegExpExecArray[] = [];
      let mainMatch: RegExpExecArray | null;

      while ((mainMatch = mainRegex.exec(relatedRecordsString)) !== null) {
        mainMatches.push(mainMatch);
      }

      let recordIdMatch: RegExpExecArray | null;
      const recordIds: string[] = [];
      while ((recordIdMatch = recordIdRegex.exec(relatedRecordsString)) !== null) {
        recordIds.push(recordIdMatch[1]);
      }

      mainMatches.forEach((match, index) => {
        if (!relationship) {
          setRelationship(match[2])
        }
        relatedRecordsArray.push({
          relationship: match[2],
          id: recordIds[index],
          class: match[3],
          name: match[4],
          actions: (
            <div className="flex items-center">
              <button
                className="text-blue-500 cursor-pointer"
                onClick={async () => {
                  const selectedRecord = await getRecord(Number(projectId), Number(recordIds[index]));
                  setSelectedRecord(selectedRecord)
                  setRecordViewModalOpen(true);
                }}
              >
                <EyeIcon className="w-4 h-4" />
              </button>
              <button 
                className="text-red-500 ml-2 cursor-pointer border rounded px-1"
                onClick={() => {
                  handleToggleToRemove(recordIds[index], match[4], match[3], 'relatedRecord')
                }} 
              >
                Remove Link
              </button>
            </div>
          ),
        });
      });

      return relatedRecordsArray;
    };

    const parsedRecords = parseRelatedRecords(relatedRecords);

    setParsedRelatedRecords(parsedRecords);
  }, [relatedRecords]);

  useEffect(() => {
    const fetchTags = async () => {
      try {
        const data = await getAllTags(Number(projectId));
        setTags(data);
      } catch (error) {
        console.error("Error fetching tags:", error);
      }
    };
    fetchTags();
  }, [projectId]);

  const handleTagSelectionChange = async (selected: string[]) => {
    if (JSON.stringify(selected) !== JSON.stringify(selectedIds)) {
      const newTags = tags.filter(tag => selected.includes(tag.id));
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

  const handleToggleToRemove = (id: string, name: string, recordName: string | undefined, type: 'tag' | 'relatedRecord') => {
    setSelectedNameToRemove(name);
    setSelectedRecordNameToRemove(recordName);
    setModalOpen(true);
    setIdToRemove(id);
    setSelectedOriginId(Number(id))
    setSelectedDestinationId(record.id)
    setConfirmationType(type)
  };

  const handleSave = async () => {
    for (const tagId of tagsToRemove) {
      await unAttachTagFromRecord(Number(projectId), Number(recordId), Number(tagId));
    }
    setSelectedTags((prevTags) => prevTags.filter(tag => !tagsToRemove.includes(tag.id)));
    setSelectedIds((prevIds) => prevIds.filter(id => !tagsToRemove.includes(id)));
    setTagsToRemove([]);
    setIsEditing(false);
    
  };

  const handleConfirmUnlink = async () => {
    if (confirmationType === 'tag') {
        if (idToRemove) {
            setTagsToRemove((prev) =>
                prev.includes(idToRemove) ? prev.filter(id => id !== idToRemove) : [...prev, idToRemove]
            );
            toast.success(`Tag ${selectedNameToRemove} removed successfully.`);
        }
    } else if (confirmationType === 'relatedRecord') {
        if (idToRemove && selectedOriginId && selectedDestinationId) {
            try {
                console.log("deleting edge")
                if (await getEdge(projectId, null, String(selectedOriginId), String(selectedDestinationId))) {
                  await deleteEdge(projectId, null, String(selectedOriginId), String(selectedDestinationId));
                }
                await queryKuzu(projectId, `MATCH (m)-[r:${relationship}]->(n) WHERE m.record_id = ${selectedOriginId} AND n.record_id = ${selectedDestinationId} DELETE r;`);
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

  const relatedRecordsColumns: Column<RelatedRecord>[] = [
    { header: "Relationship", data: "relationship" },
    { header: "ID", data: "id" },
    { header: "Class", data: "class" },
    { header: "Name", data: "name" },
    { header: "Actions", data: "actions", sortable: false },
  ];


  const parsedProperties = JSON.parse(record.properties!);
  const additionalPropertiesRows = parsedProperties
    ? Object.keys(parsedProperties).map((key) => {
        const value = parsedProperties[key as keyof object];
        return {
          label: key,
          value: typeof value === "object" ? JSON.stringify(value) : String(value),
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
              className="mt-4"
              title="Additional Properties"
              rows={additionalPropertiesRows}
            />
          </div>
          <div className="flex-grow">
            <div className="card bg-base-100 shadow-md p-2 relative mb-20">
              <div className="flex justify-between items-center">
                <h2 className="text-xl font-bold mb-4">Tags:</h2>
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
                      className="w-6 h-6 ml-2 cursor-pointer text-secondary"
                      onClick={toggleEditMode}
                    />
                  )}
                  {isEditing && (
                    <>
                      <CheckCircleIcon
                        className="w-6 h-6 ml-2 cursor-pointer text-success"
                        onClick={handleSave}
                      />
                      <XCircleIcon
                        className="w-6 h-6 ml-2 cursor-pointer text-error"
                        onClick={toggleEditMode}
                      />
                    </>
                  )}
                </div>
              </div>
              <span className="flex items-center flex-wrap mt-2">
                {selectedTags.map((tag) => (
                  <span key={tag.id} className="font-inter flex items-center border rounded-full px-2 py-1 mr-2 mb-1 flex-shrink-0" style={{ borderColor: '#07519E', color: '#07519E', font: 'Inter' }}>
                    {tag.name}
                    {!tagsToRemove.includes(tag.id) && isEditing && (
                      <XMarkIcon
                        className="w-4 h-4 ml-1 cursor-pointer text-red-600"
                        onClick={() => handleToggleToRemove(tag.id, tag.name, record?.name || "Unknown Record", 'tag')}
                      />
                    )}
                  </span>
                ))}
              </span>
            </div>
              <GenericTable
                columns={relatedRecordsColumns}
                data={parsedRelatedRecords}
                title="Related Records:"
                bordered
                searchBar={false}
                enablePagination={false}
                actionButtons={false}
                tableClassName=".table-bordered"
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
