import { createTag } from "@/app/lib/tag_services.client";
import React, { useEffect, useState } from "react";
import toast from "react-hot-toast";
import { RecordResponseDto, TagResponseDto } from "../../types/responseDTOs";
import { getRecentlyAddedRecords } from "@/app/lib/user_services.client";

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
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchRecentRecords = async () => {
      if (!projectId || projectId === "0") {
        setRecords([]);
        return;
      }

      setLoading(true);

      try {
        const recentRecords = await getRecentlyAddedRecords([projectId]);

        // Parse tags from string to array for each record
        const recordsWithParsedTags = recentRecords.map((record: any) => {
          let parsedTags = [];

          // Check if tags is a string and parse it
          if (typeof record.tags === "string") {
            try {
              parsedTags = JSON.parse(record.tags);
            } catch (e) {
              console.error("Error parsing tags for record:", record.id, e);
              parsedTags = [];
            }
          } else if (Array.isArray(record.tags)) {
            // If it's already an array, use it directly
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

  return (
    <div>
      <h3 className="font-bold mb-4">Recently Added Records</h3>

      {loading && <p className="text-sm">Loading records...</p>}

      {!loading && records.length === 0 && (
        <p className="text-base-content/70 text-sm">No recent records found</p>
      )}

      {!loading && records.length > 0 && (
        <div className="space-y-2 max-h-96 overflow-y-auto">
          <p className="text-sm text-base-content/70 mb-2">
            {records.length} recent record{records.length !== 1 ? "s" : ""}
          </p>
          <ul className="space-y-2">
            {records.map((record, index) => (
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

                    {/* Now tags should be an array */}
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
  );
};

// Default export
export default CreateTag;
