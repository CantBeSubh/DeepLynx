import React from "react";

interface Props {
  isOpen: boolean;
  isSaving: boolean;
  editingTag: boolean;
  nameInput: string;
  onNameChange: (value: string) => void;
  onCancel: () => void;
  onSave: () => void;
}

const TagEditModal: React.FC<Props> = ({
  isOpen,
  isSaving,
  editingTag,
  nameInput,
  onNameChange,
  onCancel,
  onSave,
}) => {
  if (!isOpen) return null;

  const disabled = !nameInput.trim() || isSaving;

  return (
    <div className="modal modal-open">
      <div className="modal-box max-w-md">
        <h3 className="font-bold text-lg mb-2">
          {editingTag ? "Edit Tag" : "Create Tag"}
        </h3>
        <p className="text-xs text-base-content/70 mb-4">
          Define an organization-level tag. Projects inherit this tag and can
          use it across their assets.
        </p>

        <div className="space-y-4">
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold">
                Tag Name <span className="text-error">*</span>
              </span>
            </label>
            <input
              type="text"
              className="input input-bordered input-sm"
              placeholder="e.g., PII, QA, Archive"
              value={nameInput}
              onChange={(e) => onNameChange(e.target.value)}
            />
          </div>
        </div>

        <div className="modal-action">
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            type="button"
            className="btn btn-primary btn-sm"
            disabled={disabled}
            onClick={onSave}
          >
            {isSaving ? "Saving..." : editingTag ? "Save Tag" : "Create Tag"}
          </button>
        </div>
      </div>
      <div className="modal-backdrop" onClick={onCancel} />
    </div>
  );
};

export default TagEditModal;
