import React, { useCallback, useEffect, useState } from "react";
import { TagResponseDto, RecordResponseDto } from "../../types/responseDTOs";
import { FileViewerTableRow } from "../../types/types";
import SimpleFilterInput from "../../components/SimpleFilterComponent";
import { fullTextSearch } from "@/app/lib/query_services.client";
import { attachTagToRecord } from "@/app/lib/record_services.client";
import { getRecentlyAddedRecords } from "@/app/lib/user_services.client";
import toast from "react-hot-toast";

// Helper function to parse tags - move this to the top
const parseTags = (
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

type RecordWithParsedTags = Omit<
  RecordResponseDto | FileViewerTableRow,
  "tags"
> & {
  tags: TagResponseDto[];
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
}

const AttachTags = ({
  loading,
  error,
  filteredTags,
  tags,
  searchQuery,
  onSearchChange,
  selectedTagIds,
  setSelectedTagIds,
}: Props) => {
  const handleTagToggle = (tagId: number) => {
    const newSelected = new Set(selectedTagIds);
    if (newSelected.has(tagId)) {
      newSelected.delete(tagId);
    } else {
      newSelected.add(tagId);
    }
    setSelectedTagIds(newSelected);
  };
  return (
    <div>
      <h3 className="font-bold mb-4">Attach Tags</h3>
      <SimpleFilterInput
        placeholder="Filter tags..."
        value={searchQuery}
        onChange={onSearchChange}
      />
      <div className="mt-4">
        {loading && <p>Loading tags ...</p>}
        {error && <p className="text-error flex justify-center">{error}</p>}
        {!loading && filteredTags.length === 0 && tags.length === 0 && (
          <p className="text-base-300">No Tags found</p>
        )}
        {!loading && filteredTags.length === 0 && tags.length > 0 && (
          <p className="text-base-300">No tags match your search</p>
        )}
        {!loading && filteredTags.length > 0 && (
          <div className="space-y-2">
            <p className="text-sm font-semibold">
              Tags ({filteredTags.length}):
            </p>
            <ul className="space-y-1">
              {filteredTags.map((tag, index) => (
                <li
                  key={tag.id || index}
                  className="px-3 py-1"
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
    </div>
  );
};

interface AttachTagsRecordsListProps {
  projectId: string;
  selectedTagIds: Set<number>;
  onClearSelectedTags: () => void;
}

export const AttachTagsRecordsList = ({
  projectId,
  selectedTagIds,
  onClearSelectedTags,
}: AttachTagsRecordsListProps) => {
  const [records, setRecords] = useState<RecordWithParsedTags[]>([]);
  const [searchResults, setSearchResults] = useState<RecordWithParsedTags[]>(
    []
  );
  const [loading, setLoading] = useState(false);
  const [searchLoading, setSearchLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [attachLoading, setAttachLoading] = useState(false);
  const [selectedRecordIds, setSelectedRecordIds] = useState<Set<number>>(
    new Set()
  );

  useEffect(() => {
    const fetchRecentRecords = async () => {
      if (!projectId || projectId === "0") {
        setRecords([]);
        return;
      }

      setLoading(true);

      try {
        const recentRecords = await getRecentlyAddedRecords([projectId]);

        const recordsWithParsedTags: RecordWithParsedTags[] = recentRecords.map(
          (record: RecordResponseDto) => ({
            ...record,
            tags: parseTags(
              record.tags as string | TagResponseDto[] | undefined | null
            ),
          })
        );

        setRecords(recordsWithParsedTags);
      } catch (error) {
        console.error("Error fetching recent records:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchRecentRecords();
  }, [projectId]);

  // Perform Full text search
  const performFullTextSearch = useCallback(
    async (searchTerm: string, projectId: string) => {
      if (!searchTerm.trim()) {
        setSearchResults([]);
        return;
      }

      setSearchLoading(true);

      try {
        const data = await fullTextSearch(searchTerm, [projectId]);

        const resultsWithParsedTags: RecordWithParsedTags[] = data.map(
          (record) => ({
            ...record,
            tags: parseTags(record.tags),
          })
        );

        setSearchResults(resultsWithParsedTags);
      } catch (error) {
        console.error("Search error:", error);
        setSearchResults([]);
      } finally {
        setSearchLoading(false);
      }
    },
    []
  );

  // Handle submit from search bar
  const handleSubmit = async () => {
    await performFullTextSearch(searchTerm, projectId);
  };

  // Also trigger search on Enter key
  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      handleSubmit();
    }
  };

  // Toggle record selection
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

  // Attach tags to selected records
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
      const attachPromises: Promise<TagResponseDto>[] = [];

      // For each selected record
      selectedRecordIds.forEach((recordId) => {
        // Attach each selected tag
        selectedTagIds.forEach((tagId) => {
          attachPromises.push(
            attachTagToRecord(Number(projectId), recordId, tagId)
          );
        });
      });

      await Promise.all(attachPromises);

      toast.success(
        `Successfully attached ${selectedTagIds.size} tag(s) to ${selectedRecordIds.size} record(s)`
      );

      // Clear selections and refresh records
      setSelectedRecordIds(new Set());
      onClearSelectedTags();

      // Refresh the records list to show updated tags
      if (searchTerm.trim()) {
        await performFullTextSearch(searchTerm, projectId);
      } else {
        const recentRecords = await getRecentlyAddedRecords([projectId]);
        const recordsWithParsedTags: RecordWithParsedTags[] = recentRecords.map(
          (record: RecordResponseDto) => ({
            ...record,
            tags: parseTags(
              record.tags as string | TagResponseDto[] | undefined | null
            ),
          })
        );
        setRecords(recordsWithParsedTags);
      }
    } catch (error) {
      console.error("Error attaching tags:", error);
      toast.error("Failed to attach tags to some records");
    } finally {
      setAttachLoading(false);
    }
  };

  // Determine which records to display
  const displayRecords = searchTerm.trim() ? searchResults : records;
  const isSearching = searchTerm.trim().length > 0;

  return (
    <div
      className="w-[85%] mx-auto flex flex-col"
      style={{ height: "calc(90vh - 200px)" }}
    >
      <div className="gap-2 mb-4">
        <h3 className="font-bold mb-4">Search Records</h3>
        <input
          type="text"
          placeholder="Search Record"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyDown={handleKeyPress}
          className="input input-bordered mb-4 w-full"
        />
        {/* Attach Tags Button */}
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

      {/* Search Results or Recent Records */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <h3 className="font-bold mb-4">
          {isSearching ? "Search Results" : "Recently Added Records"}
        </h3>

        {(searchLoading || loading) && (
          <p className="text-sm">Loading records...</p>
        )}

        {!searchLoading && !loading && displayRecords.length === 0 && (
          <p className="text-base-content/70 text-sm">
            {isSearching
              ? "No records found matching your search"
              : "No recent records found"}
          </p>
        )}

        {!searchLoading && !loading && displayRecords.length > 0 && (
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
                    <input
                      type="checkbox"
                      className="checkbox checkbox-primary mt-1"
                      checked={
                        record.id !== null && selectedRecordIds.has(record.id)
                      }
                      onChange={() => handleRecordToggle(record.id)}
                      onClick={(e) => e.stopPropagation()}
                    />

                    <div className="flex-1">
                      <div className="text-sm font-semibold">{record.name}</div>

                      {record.tags && record.tags.length > 0 && (
                        <div className="flex gap-1 flex-wrap mt-1">
                          {record.tags.map((tag) => (
                            <span
                              className="badge badge-outline badge-secondary badge-sm"
                              key={tag.id}
                            >
                              {tag.name}
                            </span>
                          ))}
                        </div>
                      )}

                      {record.lastUpdatedAt && (
                        <div className="text-xs text-base-content/60 mt-1">
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
    </div>
  );
};

export default AttachTags;
