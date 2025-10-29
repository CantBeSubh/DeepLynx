import { createTag } from "@/app/lib/tag_services.client";
import React, { useCallback, useEffect, useState } from "react";
import toast from "react-hot-toast";
import { RecordResponseDto, TagResponseDto } from "../../types/responseDTOs";
import { getRecentlyAddedRecords } from "@/app/lib/user_services.client";
import SearchBar from "../../components/SearchBar";
import { fullTextSearch } from "@/app/lib/query_services.client";
import { FileViewerTableRow } from "../../types/types";

// Main CreateTag Component
interface CreateTagProps {
  projectId: string;
  onTagCreated: () => Promise<void>;
}

const CreateTag = ({ projectId, onTagCreated }: CreateTagProps) => {
  const [tagName, setTagName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [createdTags, setCreatedTags] = useState<TagResponseDto[]>([]);
  const [selectedTagIds, setSelectedTagIds] = useState<Set<number>>(new Set());

  const handleCreateTag = async () => {
    if (!tagName.trim()) {
      setError("Tag name cannot be empty");
      return;
    }

    if (!projectId || projectId === "0") {
      setError("Invalid project selected");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const newTag = await createTag(Number(projectId), { name: tagName });
      setTagName("");
      toast.success("Tag Created");

      setCreatedTags((prev) => [...prev, newTag]);
      await onTagCreated();
    } catch (err) {
      setError("Failed to create tag");
      console.error("Error creating tag:", err);
      toast.error("Failed to create tag.");
    } finally {
      setLoading(false);
    }
  };

  const handleTagToggle = (tagId: number) => {
    const newSelected = new Set(selectedTagIds);
    if (newSelected.has(tagId)) {
      newSelected.delete(tagId);
    } else {
      newSelected.add(tagId);
    }
    setSelectedTagIds(newSelected);
  };

  const isProjectSelected = projectId && projectId !== "0";

  return (
    <div className="w-[70%] mx-auto">
      <div>
        <h3 className="font-bold mb-4">Name</h3>
        <input
          type="text"
          placeholder="Example: Reactor"
          className="input input-bordered w-full mb-4"
          value={tagName}
          onChange={(e) => setTagName(e.target.value)}
          disabled={loading || !isProjectSelected}
          onKeyDown={(e) => {
            if (
              e.key === "Enter" &&
              !loading &&
              tagName.trim() &&
              isProjectSelected
            ) {
              handleCreateTag();
            }
          }}
        />
        {!isProjectSelected && (
          <p className="text-primary text-sm mb-2">
            Please select a project first
          </p>
        )}
        <div className="flex justify-end mb-6">
          <button
            className="btn btn-primary"
            onClick={handleCreateTag}
            disabled={loading || !tagName.trim() || !isProjectSelected}
          >
            {loading ? "Creating..." : "Create Tag"}
          </button>
        </div>

        {/* Newly Created Tags List */}
        {createdTags.length > 0 && (
          <div className="mt-6 border-t pt-4">
            <h4 className="font-semibold mb-3">Created Tags</h4>
            <div className="space-y-2 max-h-64 overflow-y-auto">
              <p className="text-sm text-base-content/70 mb-2">
                Select tags to attach to records ({selectedTagIds.size}{" "}
                selected)
              </p>
              <ul className="space-y-1">
                {createdTags.map((tag) => (
                  <li
                    key={tag.id}
                    className="flex items-center px-3 py-2 hover:bg-base-200 rounded cursor-pointer"
                    onClick={() => handleTagToggle(tag.id)}
                  >
                    <input
                      type="checkbox"
                      className="checkbox checkbox-primary"
                      checked={selectedTagIds.has(tag.id)}
                      onChange={() => handleTagToggle(tag.id)}
                    />
                    <span className="badge ml-2">{tag.name}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

// Recently Added Records Component
interface CreateTagRecordsListProps {
  projectId: string;
}

export const CreateTagRecordsList = ({
  projectId,
}: CreateTagRecordsListProps) => {
  const [records, setRecords] = useState<RecordResponseDto[]>([]);
  const [searchResults, setSearchResults] = useState<any[]>([]); // Changed type
  const [loading, setLoading] = useState(false);
  const [searchLoading, setSearchLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    const fetchRecentRecords = async () => {
      if (!projectId || projectId === "0") {
        setRecords([]);
        return;
      }

      setLoading(true);

      try {
        const recentRecords = await getRecentlyAddedRecords([projectId]);

        const recordsWithParsedTags = recentRecords.map((record: any) => {
          let parsedTags = [];

          if (typeof record.tags === "string") {
            try {
              parsedTags = JSON.parse(record.tags);
            } catch (e) {
              console.error("Error parsing tags for record:", record.id, e);
              parsedTags = [];
            }
          } else if (Array.isArray(record.tags)) {
            parsedTags = record.tags;
          }

          return {
            ...record,
            tags: parsedTags,
          };
        });

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
        console.log("Searching for:", searchTerm, "in project:", projectId);
        const data = await fullTextSearch(searchTerm, [projectId]);
        console.log("Search results:", data);

        // Parse tags for search results too
        const resultsWithParsedTags = data.map((record: any) => {
          let parsedTags = [];

          if (typeof record.tags === "string") {
            try {
              parsedTags = JSON.parse(record.tags);
            } catch (e) {
              console.error("Error parsing tags for record:", record.id, e);
              parsedTags = [];
            }
          } else if (Array.isArray(record.tags)) {
            parsedTags = record.tags;
          }

          return {
            ...record,
            tags: parsedTags,
          };
        });

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

  // Determine which records to display
  const displayRecords = searchTerm.trim() ? searchResults : records;
  const isSearching = searchTerm.trim().length > 0;

  return (
    <div className="w-[85%] mx-auto">
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
        <div className="flex justify-end">
          <button
            className="btn btn-primary"
            onClick={handleSubmit}
            disabled={searchLoading || !searchTerm.trim()}
          >
            {searchLoading ? "Searching..." : "Search"}
          </button>
        </div>
      </div>

      {/* Search Results or Recent Records */}
      <div className="mt-6">
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
          <div className="space-y-2 max-h-96 overflow-y-auto">
            <p className="text-sm text-base-content/70 mb-2">
              {displayRecords.length} record
              {displayRecords.length !== 1 ? "s" : ""}
            </p>
            <ul className="space-y-2">
              {displayRecords.map((record, index) => (
                <li
                  key={record.id || index}
                  className="px-3 py-2 hover:bg-info/50 cursor-pointer transition-colors border-b border-base-200"
                >
                  <div className="flex items-start gap-2">
                    <input
                      type="checkbox"
                      className="checkbox checkbox-primary mt-1"
                    />

                    <div className="flex-1">
                      <div className="text-sm font-semibold">{record.name}</div>

                      {record.tags &&
                        Array.isArray(record.tags) &&
                        record.tags.length > 0 && (
                          <div className="flex gap-1 flex-wrap mt-1">
                            {record.tags.map((tag: any) => (
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

// Default export
export default CreateTag;
