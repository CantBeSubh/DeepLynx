import React, { useState } from "react";
import { TagResponseDto } from "../../types/responseDTOs";
import toast from "react-hot-toast";
import SimpleFilterInput from "../../components/SimpleFilterComponent";

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

const EditTags = ({
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
  // Get the actual tag objects for the selected IDs
  const selectedTags = tags.filter((tag) => selectedTagIds.has(tag.id));

  // Remove the search functionality from this component since we're editing
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
    <div
      className="w-[85%] mx-auto flex flex-col"
      style={{ height: "calc(90vh - 200px)" }}
    >
      <h3 className="font-bold mb-4">Select Tags to Edit</h3>

      {/* Filter tags input */}
      <SimpleFilterInput
        placeholder="Filter tags..."
        value={searchQuery}
        onChange={onSearchChange}
      />

      {/* Display filtered tags */}
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
    </div>
  );
};

interface EditTagsNameFieldsProps {
  selectedTags: TagResponseDto[];
  onUpdateTag: (tagId: number, newName: string) => Promise<void>;
}

export const EditTagsNameFields = ({
  selectedTags,
  onUpdateTag,
}: EditTagsNameFieldsProps) => {
  const [editedNames, setEditedNames] = useState<Map<number, string>>(
    new Map()
  );
  const [savingTagId, setSavingTagId] = useState<number | null>(null);

  // Initialize edited names when selected tags change
  React.useEffect(() => {
    const initialNames = new Map<number, string>();
    selectedTags.forEach((tag) => {
      initialNames.set(tag.id, tag.name);
    });
    setEditedNames(initialNames);
  }, [selectedTags]);

  const handleNameChange = (tagId: number, newName: string) => {
    const newMap = new Map(editedNames);
    newMap.set(tagId, newName);
    setEditedNames(newMap);
  };

  const handleSaveTag = async (tag: TagResponseDto) => {
    const newName = editedNames.get(tag.id);

    if (!newName || !newName.trim()) {
      toast.error("Tag name cannot be empty");
      return;
    }

    if (newName === tag.name) {
      toast.error("No changes to save");
      return;
    }

    setSavingTagId(tag.id);
    try {
      await onUpdateTag(tag.id, newName);
      toast.success(`Tag "${tag.name}" updated to "${newName}"`);
    } catch (error) {
      console.error("Error updating tag:", error);
      toast.error("Failed to update tag");
      // Reset to original name on error
      const newMap = new Map(editedNames);
      newMap.set(tag.id, tag.name);
      setEditedNames(newMap);
    } finally {
      setSavingTagId(null);
    }
  };

  const handleReset = (tag: TagResponseDto) => {
    const newMap = new Map(editedNames);
    newMap.set(tag.id, tag.name);
    setEditedNames(newMap);
  };

  if (selectedTags.length === 0) {
    return (
      <div
        className="w-[85%] mx-auto flex items-center justify-center"
        style={{ height: "calc(90vh - 200px)" }}
      >
        <p className="text-base-content/70 text-sm">
          Select tags to edit their names
        </p>
      </div>
    );
  }

  return (
    <div
      className="w-[85%] mx-auto flex flex-col"
      style={{ height: "calc(90vh - 200px)" }}
    >
      <h3 className="font-bold mb-4">Edit Tag Names</h3>
      <p className="text-sm text-base-content/70 mb-4">
        {selectedTags.length} tag{selectedTags.length !== 1 ? "s" : ""} selected
      </p>

      <div className="space-y-4 overflow-y-auto flex-1">
        {selectedTags.map((tag) => {
          const isEdited = editedNames.get(tag.id) !== tag.name;
          const isSaving = savingTagId === tag.id;

          return (
            <div
              key={tag.id}
              className="card bg-base-200 shadow-sm p-4 space-y-3"
            >
              <div className="flex items-center gap-2">
                <span className="text-xs text-base-content/60">Original:</span>
                <span className="badge badge-secondary">{tag.name}</span>
              </div>

              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">New Name</span>
                </label>
                <input
                  type="text"
                  className="input input-bordered w-full"
                  value={editedNames.get(tag.id) || ""}
                  onChange={(e) => handleNameChange(tag.id, e.target.value)}
                  disabled={isSaving}
                  placeholder="Enter new tag name"
                />
              </div>

              <div className="flex gap-2 justify-end">
                <button
                  className="btn btn-ghost btn-sm"
                  onClick={() => handleReset(tag)}
                  disabled={!isEdited || isSaving}
                >
                  Reset
                </button>
                <button
                  className="btn btn-primary btn-sm"
                  onClick={() => handleSaveTag(tag)}
                  disabled={!isEdited || isSaving}
                >
                  {isSaving ? (
                    <>
                      <span className="loading loading-spinner loading-xs"></span>
                      Saving...
                    </>
                  ) : (
                    "Save Changes"
                  )}
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default EditTags;
