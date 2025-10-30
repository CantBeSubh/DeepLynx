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
  const [savingAll, setSavingAll] = useState(false);

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

  const handleSaveAll = async () => {
    // Get all tags that have been edited
    const editsToSave = selectedTags.filter((tag) => {
      const newName = editedNames.get(tag.id);
      return newName && newName.trim() && newName !== tag.name;
    });

    if (editsToSave.length === 0) {
      toast.error("No changes to save");
      return;
    }

    // Check for empty names
    const hasEmptyName = editsToSave.some((tag) => {
      const newName = editedNames.get(tag.id);
      return !newName || !newName.trim();
    });

    if (hasEmptyName) {
      toast.error("Tag names cannot be empty");
      return;
    }

    setSavingAll(true);
    const successfulUpdates: string[] = [];
    const failedUpdates: string[] = [];

    try {
      // Update all edited tags
      for (const tag of editsToSave) {
        const newName = editedNames.get(tag.id)!;
        try {
          await onUpdateTag(tag.id, newName);
          successfulUpdates.push(`"${tag.name}" → "${newName}"`);
        } catch (error) {
          failedUpdates.push(tag.name);
          console.error(`Failed to update tag ${tag.name}:`, error);
        }
      }

      // Show results
      if (successfulUpdates.length > 0) {
        toast.success(
          `Successfully updated ${successfulUpdates.length} tag${
            successfulUpdates.length !== 1 ? "s" : ""
          }`
        );
      }

      if (failedUpdates.length > 0) {
        toast.error(
          `Failed to update ${failedUpdates.length} tag${
            failedUpdates.length !== 1 ? "s" : ""
          }: ${failedUpdates.join(", ")}`
        );
      }
    } finally {
      setSavingAll(false);
    }
  };

  const handleResetAll = () => {
    const initialNames = new Map<number, string>();
    selectedTags.forEach((tag) => {
      initialNames.set(tag.id, tag.name);
    });
    setEditedNames(initialNames);
    toast.error("All changes reset");
  };

  // Check if any tag has been edited
  const hasAnyEdits = selectedTags.some((tag) => {
    const editedName = editedNames.get(tag.id);
    return editedName !== tag.name;
  });

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
      <div className="mb-4">
        <h3 className="font-bold mb-2">Edit Tag Names</h3>
        <p className="text-sm text-base-content/70">
          {selectedTags.length} tag{selectedTags.length !== 1 ? "s" : ""}{" "}
          selected
        </p>
      </div>

      <div className="space-y-4 overflow-y-auto flex-1">
        {selectedTags.map((tag) => {
          const isEdited = editedNames.get(tag.id) !== tag.name;

          return (
            <div
              key={tag.id}
              className={"card bg-base-200/50 shadow-sm p-4 space-y-3"}
            >
              <div className="flex items-center gap-2">
                <span className="text-xs text-base-content/60">Original:</span>
                <span className="badge badge-secondary">{tag.name}</span>
                {isEdited && (
                  <span className="badge badge-primary badge-sm">Modified</span>
                )}
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
                  disabled={savingAll}
                  placeholder="Enter new tag name"
                />
              </div>
            </div>
          );
        })}
      </div>

      {/* Action buttons at the bottom */}
      {hasAnyEdits && (
        <div className="mt-4 flex gap-2 justify-end p-3 rounded-lg">
          <button
            className="btn btn-ghost"
            onClick={handleResetAll}
            disabled={savingAll}
          >
            Reset All
          </button>
          <button
            className="btn btn-primary"
            onClick={handleSaveAll}
            disabled={savingAll}
          >
            {savingAll ? (
              <>
                <span className="loading loading-spinner loading-sm"></span>
                Saving Changes...
              </>
            ) : (
              "Save All Changes"
            )}
          </button>
        </div>
      )}
    </div>
  );
};

export default EditTags;
