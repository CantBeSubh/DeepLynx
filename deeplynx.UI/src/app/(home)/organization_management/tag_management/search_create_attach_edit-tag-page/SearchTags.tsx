import SimpleFilterInput from "@/app/(home)/components/SimpleFilterComponent";
import {
  RecordResponseDto,
  TagResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  attachTagToRecord,
  unattachTagFromRecord,
} from "@/app/lib/client_service/record_services.client";
import { LinkSlashIcon } from "@heroicons/react/24/outline";
import React, { useState } from "react";
import toast from "react-hot-toast";

export const parseTags = (
  tags: string | TagResponseDto[] | undefined | null
): TagResponseDto[] => {
  if (!tags) return [];

  if (typeof tags === "string") {
    try {
      return JSON.parse(tags) as TagResponseDto[];
    } catch (e) {
      console.error("Error parsing tags:", e);
      return [];
    }
  }

  if (Array.isArray(tags)) {
    return tags;
  }

  return [];
};

interface Props {
  loading: boolean;
  error: string | null;
  filteredTags: TagResponseDto[];
  tags: TagResponseDto[];
  searchQuery: string;
  onSearchChange: (query: string) => void;
  selectedTagIds: Set<number>;
  setSelectedTagIds: React.Dispatch<React.SetStateAction<Set<number>>>;
  projectId: string;
  onSearchByTags: (tagIds: number[]) => Promise<void>;
}

const SearchTags = ({
  loading,
  error,
  filteredTags,
  tags,
  searchQuery,
  onSearchChange,
  selectedTagIds,
  setSelectedTagIds,
  projectId,
  onSearchByTags,
}: Props) => {
  const [searchLoading, setSearchLoading] = useState(false);

  const handleTagToggle = (tagId: number) => {
    const newSelected = new Set(selectedTagIds);
    if (newSelected.has(tagId)) {
      newSelected.delete(tagId);
    } else {
      newSelected.add(tagId);
    }
    setSelectedTagIds(newSelected);
  };

  const handleSearchByTags = async () => {
    if (selectedTagIds.size === 0) {
      toast.error("Please select at least one tag to search");
      return;
    }

    setSearchLoading(true);
    try {
      await onSearchByTags(Array.from(selectedTagIds));
    } catch (error) {
      console.error("Error searching by tags:", error);
      toast.error("Failed to search records");
    } finally {
      setSearchLoading(false);
    }
  };

  return (
    <div
      className="w-[85%] mx-auto flex flex-col"
      style={{ height: "calc(90vh - 325px)" }}
    >
      <h3 className="font-bold mb-4">Search Tags</h3>

      <SimpleFilterInput
        placeholder="Filter tags..."
        value={searchQuery}
        onChange={onSearchChange}
      />

      <div className="mt-4 flex-1 flex flex-col overflow-hidden">
        {loading && <p>Loading tags ...</p>}
        {error && <p className="text-error flex justify-center">{error}</p>}
        {!loading && filteredTags.length === 0 && tags.length === 0 && (
          <p className="text-base-300">No Tags found</p>
        )}
        {!loading && filteredTags.length === 0 && tags.length > 0 && (
          <p className="text-base-300">No tags match your search</p>
        )}
        {!loading && filteredTags.length > 0 && (
          <div className="space-y-2 flex-1 flex flex-col overflow-hidden">
            <div className="flex justify-between items-center mb-2">
              <p className="text-sm font-semibold">
                Tags ({filteredTags.length}):
              </p>
              {selectedTagIds.size > 0 && (
                <span className="text-xs text-base-content/70">
                  {selectedTagIds.size} selected
                </span>
              )}
            </div>
            <ul className="space-y-1 flex-1 overflow-y-auto">
              {filteredTags.map((tag, index) => (
                <li
                  key={tag.id || index}
                  className="px-3 py-1 hover:bg-base-200 rounded cursor-pointer"
                  onClick={() => handleTagToggle(tag.id)}
                >
                  <input
                    type="checkbox"
                    className="checkbox checkbox-primary"
                    checked={selectedTagIds.has(tag.id)}
                    onChange={() => handleTagToggle(tag.id)}
                  />
                  <span className="badge ml-2">
                    {tag.name || JSON.stringify(tag)}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {selectedTagIds.size > 0 && (
        <div className="mt-4">
          <button
            className="btn btn-primary w-full"
            onClick={handleSearchByTags}
            disabled={searchLoading}
          >
            {searchLoading ? (
              <span className="loading loading-spinner loading-sm"></span>
            ) : (
              `Search Records with ${selectedTagIds.size} Tag${
                selectedTagIds.size > 1 ? "s" : ""
              }`
            )}
          </button>
        </div>
      )}
    </div>
  );
};

interface SearchTagsRecordsListProps {
  projectId: string;
  selectedTagIds: Set<number>;
  onClearSelectedTags: () => void;
  recordsFromTagSearch: RecordResponseDto[];
  isSearchingByTags: boolean;
  onClearSearch: () => void;
  onRefreshSearch: () => Promise<void>;
}

export const SearchTagsRecordsList = ({
  projectId,
  selectedTagIds,
  onClearSelectedTags,
  recordsFromTagSearch,
  isSearchingByTags,
  onClearSearch,
  onRefreshSearch,
}: SearchTagsRecordsListProps) => {
  const [attachLoading, setAttachLoading] = useState(false);
  const [selectedRecordIds, setSelectedRecordIds] = useState<Set<number>>(
    new Set()
  );
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [recordToUnattach, setRecordToUnattach] =
    useState<RecordResponseDto | null>(null);
  const [unattachLoading, setUnattachLoading] = useState(false);
  const { organization, hasLoaded } = useOrganizationSession();

  const handleRecordToggle = (recordId: number | null) => {
    if (recordId === null) return;

    const newSelected = new Set(selectedRecordIds);
    if (newSelected.has(recordId)) {
      newSelected.delete(recordId);
    } else {
      newSelected.add(recordId);
    }
    setSelectedRecordIds(newSelected);
  };

  const handleAttachTags = async () => {
    if (selectedRecordIds.size === 0) {
      toast.error("Please select at least one record");
      return;
    }

    if (selectedTagIds.size === 0) {
      toast.error("Please select at least one tag to attach");
      return;
    }

    setAttachLoading(true);

    try {
      const attachPromises: Promise<{ message: string }>[] = [];

      selectedRecordIds.forEach((recordId) => {
        selectedTagIds.forEach((tagId) => {
          attachPromises.push(
            attachTagToRecord(
              organization?.organizationId as number,
              Number(projectId),
              recordId,
              tagId
            )
          );
        });
      });

      // Then await them
      await Promise.all(attachPromises);

      toast.success(
        `Successfully attached ${selectedTagIds.size} tag(s) to ${selectedRecordIds.size} record(s)`
      );

      setSelectedRecordIds(new Set());
      onClearSelectedTags();
    } catch (error) {
      console.error("Error attaching tags:", error);
      toast.error("Failed to attach tags to some records");
    } finally {
      setAttachLoading(false);
    }
  };

  const handleClearSearch = () => {
    onClearSearch();
    setSelectedRecordIds(new Set());
    toast.success("Search cleared");
  };

  const handleOpenUnattachModal = (record: RecordResponseDto) => {
    setRecordToUnattach(record);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setRecordToUnattach(null);
  };

  const handleUnattachTags = async () => {
    if (!recordToUnattach || selectedTagIds.size === 0) {
      return;
    }

    if (recordToUnattach.id === null) {
      toast.error("Invalid record ID");
      handleCloseModal();
      return;
    }

    setUnattachLoading(true);

    try {
      const unattachPromises: Promise<{ message: string }>[] = [];

      const tagsToRemove = recordToUnattach.tags
        ?.filter((tag) => tag.id !== null && selectedTagIds.has(tag.id))
        .map((tag) => tag.id) as number[];

      if (tagsToRemove.length === 0) {
        toast.error("No selected tags found on this record");
        handleCloseModal();
        return;
      }

      tagsToRemove.forEach((tagId) => {
        unattachPromises.push(
          unattachTagFromRecord(
            organization?.organizationId as number,
            Number(projectId),
            recordToUnattach.id as number,
            tagId
          )
        );
      });

      await Promise.all(unattachPromises);

      toast.success(
        `Successfully removed ${tagsToRemove.length} tag${
          tagsToRemove.length !== 1 ? "s" : ""
        } from "${recordToUnattach.name}"`
      );

      handleCloseModal();

      if (onRefreshSearch) {
        await onRefreshSearch();
      }
    } catch (error) {
      console.error("Error unattaching tags:", error);
      toast.error("Failed to remove tags from record");
    } finally {
      setUnattachLoading(false);
    }
  };

  const displayRecords = recordsFromTagSearch;

  return (
    <div
      className="w-[85%] mx-auto flex flex-col"
      style={{ height: "calc(90vh - 325px)" }}
    >
      <div className="gap-2 mb-4">
        <h3 className="font-bold mb-4">Records with Selected Tags</h3>

        {selectedRecordIds.size > 0 && (
          <div className="mb-4 p-3 bg-base-200 rounded-lg flex items-center justify-between">
            <span className="text-sm">
              {selectedRecordIds.size} record(s) selected •{" "}
              {selectedTagIds.size} tag(s) to attach
            </span>
            <button
              className="btn btn-secondary btn-sm"
              onClick={handleAttachTags}
              disabled={attachLoading || selectedTagIds.size === 0}
            >
              {attachLoading ? "Attaching..." : "Attach Tags"}
            </button>
          </div>
        )}
      </div>

      <div className="flex-1 flex flex-col overflow-hidden">
        {isSearchingByTags && <p className="text-sm">Loading records...</p>}

        {!isSearchingByTags && displayRecords.length === 0 && (
          <p className="text-base-content/70 text-sm">
            Select tags and click "Search Records" to find records
          </p>
        )}

        {!isSearchingByTags && displayRecords.length > 0 && (
          <div className="flex-1 flex flex-col overflow-hidden">
            <p className="text-sm text-base-content/70 mb-2">
              {displayRecords.length} record
              {displayRecords.length !== 1 ? "s" : ""}
            </p>
            <ul className="space-y-2 overflow-y-auto flex-1">
              {displayRecords.map((record, index) => (
                <li
                  key={record.id || index}
                  className="px-3 py-2 hover:bg-info/50 cursor-pointer transition-colors border-b border-base-200"
                >
                  <div className="flex items-start gap-2">
                    <div className="flex-1">
                      <div className="text-sm font-semibold">{record.name}</div>

                      {record.tags && record.tags.length > 0 && (
                        <div className="flex gap-1 flex-wrap mt-1 items-center">
                          {record.tags.map((tag) => {
                            const isSearchedTag =
                              tag.id !== null && selectedTagIds.has(tag.id);
                            return (
                              <span
                                className={`badge badge-sm ${
                                  isSearchedTag
                                    ? "badge-secondary"
                                    : "badge-outline badge-secondary"
                                }`}
                                key={tag.id}
                              >
                                {tag.name}
                              </span>
                            );
                          })}
                          <div className="ml-auto">
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                handleOpenUnattachModal(record);
                              }}
                              className="btn btn-ghost btn-sm p-1"
                              title="Remove selected tags from this record"
                            >
                              <LinkSlashIcon className="size-6 text-error" />
                            </button>
                          </div>
                        </div>
                      )}

                      {record.lastUpdatedAt && (
                        <div className="text-xs text-base-content/60 mt-1">
                          Last Updated:{" "}
                          {new Date(record.lastUpdatedAt).toLocaleDateString()}
                        </div>
                      )}
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {displayRecords.length > 0 && (
        <div className="mt-4">
          <button
            className="btn btn-outline btn-error w-full"
            onClick={handleClearSearch}
          >
            Clear Search Results
          </button>
        </div>
      )}

      {isModalOpen && recordToUnattach && (
        <div className="modal modal-open">
          <div className="modal-box">
            <h3 className="font-bold text-lg">Remove Tags from Record</h3>
            <p className="py-4">
              Are you sure you want to remove {selectedTagIds.size} selected tag
              {selectedTagIds.size !== 1 ? "s" : ""} from "
              {recordToUnattach.name}"?
            </p>

            <div className="mb-4">
              <p className="text-sm font-semibold mb-2">Tags to be removed:</p>
              <div className="flex gap-1 flex-wrap">
                {recordToUnattach.tags
                  ?.filter(
                    (tag) => tag.id !== null && selectedTagIds.has(tag.id)
                  )
                  .map((tag) => (
                    <span
                      className="badge badge-secondary badge-sm"
                      key={tag.id}
                    >
                      {tag.name}
                    </span>
                  ))}
              </div>
            </div>

            <div className="modal-action">
              <button className="btn btn-ghost" onClick={handleCloseModal}>
                Cancel
              </button>
              <button
                className="btn btn-error"
                onClick={handleUnattachTags}
                disabled={unattachLoading}
              >
                {unattachLoading ? (
                  <>
                    <span className="loading loading-spinner loading-sm"></span>
                    Removing...
                  </>
                ) : (
                  "Remove Tags"
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchTags;
