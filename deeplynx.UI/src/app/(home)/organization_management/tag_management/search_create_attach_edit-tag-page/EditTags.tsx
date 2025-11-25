import SimpleFilterInput from "@/app/(home)/components/SimpleFilterComponent";
import { TagResponseDto } from "@/app/(home)/types/responseDTOs";
import { InformationCircleIcon } from "@heroicons/react/24/outline";
import React, { useState } from "react";
import toast from "react-hot-toast";

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

const EditTags = ({
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
    <div
      className="w-[85%] mx-auto flex flex-col"
      style={{ height: "calc(90vh - 325px)" }}
    >
      <h3 className="font-bold mb-4">Select Tags to Edit</h3>

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
    </div>
  );
};

interface EditTagsNameFieldsProps {
  selectedTags: TagResponseDto[];
  onUpdateTag: (tagId: number, newName: string) => Promise<void>;
  onArchiveTag: (tagId: number) => Promise<void>;
}

export const EditTagsNameFields = ({
  selectedTags,
  onUpdateTag,
  onArchiveTag,
}: EditTagsNameFieldsProps) => {
  const [editedNames, setEditedNames] = useState<Map<number, string>>(
    new Map()
  );
  const [savingAll, setSavingAll] = useState(false);
  const [isArchiveModalOpen, setIsArchiveModalOpen] = useState(false);
  const [tagToArchive, setTagToArchive] = useState<TagResponseDto | null>(null);
  const [archiving, setArchiving] = useState(false);

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
    const editsToSave = selectedTags.filter((tag) => {
      const newName = editedNames.get(tag.id);
      return newName && newName.trim() && newName !== tag.name;
    });

    if (editsToSave.length === 0) {
      toast.error("No changes to save");
      return;
    }

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
    toast.success("All changes reset");
  };

  const handleOpenArchiveModal = (tag: TagResponseDto) => {
    setTagToArchive(tag);
    setIsArchiveModalOpen(true);
  };

  const handleCloseArchiveModal = () => {
    setIsArchiveModalOpen(false);
    setTagToArchive(null);
  };

  const handleArchiveTag = async () => {
    if (!tagToArchive) return;

    setArchiving(true);
    try {
      await onArchiveTag(tagToArchive.id);
      toast.success(`Tag "${tagToArchive.name}" archived successfully`);
      handleCloseArchiveModal();
    } catch (error) {
      console.error("Error archiving tag:", error);
      toast.error(`Failed to archive tag "${tagToArchive.name}"`);
    } finally {
      setArchiving(false);
    }
  };

  const hasAnyEdits = selectedTags.some((tag) => {
    const editedName = editedNames.get(tag.id);
    return editedName !== tag.name;
  });

  if (selectedTags.length === 0) {
    return (
      <div
        className="w-[85%] mx-auto flex items-center justify-center"
        style={{ height: "calc(90vh - 325px)" }}
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
              className="card bg-base-200/50 shadow-sm p-4 space-y-3"
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="text-xs text-base-content/60">
                    Original:
                  </span>
                  <span className="badge badge-secondary">{tag.name}</span>
                  {isEdited && (
                    <span className="badge badge-primary badge-sm">
                      Modified
                    </span>
                  )}
                </div>
                <button
                  className="btn btn-warning btn-sm"
                  onClick={() => handleOpenArchiveModal(tag)}
                  disabled={savingAll}
                  title="Archive this tag"
                >
                  Archive
                </button>
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

      {isArchiveModalOpen && tagToArchive && (
        <div className="modal modal-open">
          <div className="modal-box">
            <h3 className="font-bold text-lg">Archive Tag</h3>
            <p className="py-4">
              Are you sure you want to archive the tag{" "}
              <span className="font-semibold">"{tagToArchive.name}"</span>?
            </p>
            <p className="text-sm flex items-center gap-2">
              <InformationCircleIcon className="size-6" /> Archived tags can be
              restored later if needed.
            </p>

            <div className="modal-action">
              <button
                className="btn btn-ghost"
                onClick={handleCloseArchiveModal}
                disabled={archiving}
              >
                Cancel
              </button>
              <button
                className="btn btn-warning"
                onClick={handleArchiveTag}
                disabled={archiving}
              >
                {archiving ? (
                  <>
                    <span className="loading loading-spinner loading-sm"></span>
                    Archiving...
                  </>
                ) : (
                  "Archive Tag"
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default EditTags;
