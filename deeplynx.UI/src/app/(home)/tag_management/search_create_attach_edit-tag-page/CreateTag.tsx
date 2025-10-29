import { createTag } from "@/app/lib/tag_services.client";
import React, { useState } from "react";
import toast from "react-hot-toast";
import { TagResponseDto } from "../../types/responseDTOs";

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

      // Add the newly created tag to the list
      setCreatedTags((prev) => [...prev, newTag]);

      // Refresh the tags list from parent
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

export default CreateTag;
