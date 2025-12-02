import React from "react";

type Mode = "tag" | "label";

interface Props {
  isOpen: boolean;
  isSaving: boolean;
  mode: Mode;
  editing: boolean;
  nameInput: string;
  descriptionInput: string;
  onNameChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onCancel: () => void;
  onSave: () => void;
}

const ProjectTagEditModal: React.FC<Props> = ({
  isOpen,
  isSaving,
  mode,
  editing,
  nameInput,
  descriptionInput,
  onNameChange,
  onDescriptionChange,
  onCancel,
  onSave,
}) => {
  if (!isOpen) return null;

  const disabled = !nameInput.trim() || isSaving;
  const isLabel = mode === "label";

  return (
    <div className="modal modal-open">
      <div className="modal-box max-w-md">
        <h3 className="font-bold text-lg mb-2">
          {editing
            ? isLabel
              ? "Edit Security Label"
              : "Edit Tag"
            : isLabel
            ? "Create Security Label"
            : "Create Tag"}
        </h3>
        <p className="text-xs text-base-content/70 mb-4">
          {isLabel
            ? "Define a project-level security label for attribute-based access control."
            : "Define a project-level tag. This project also inherits tags defined at the organization level."}
        </p>

        <div className="space-y-4">
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold">
                {isLabel ? "Label Name" : "Tag Name"}{" "}
                <span className="text-error">*</span>
              </span>
            </label>
            <input
              type="text"
              className="input input-bordered input-sm"
              placeholder={
                isLabel
                  ? "e.g., CUI, Export Controlled"
                  : "e.g., PII, QA, Archive"
              }
              value={nameInput}
              onChange={(e) => onNameChange(e.target.value)}
            />
          </div>

          {isLabel && (
            <div className="form-control">
              <label className="label">
                <span className="label-text font-semibold text-xs">
                  Description
                </span>
              </label>
              <textarea
                className="textarea textarea-bordered textarea-sm"
                placeholder="Optional description for this security label."
                rows={3}
                value={descriptionInput}
                onChange={(e) => onDescriptionChange(e.target.value)}
              />
            </div>
          )}
        </div>

        <div className="modal-action">
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={onCancel}
            disabled={isSaving}
          >
            Cancel
          </button>
          <button
            type="button"
            className="btn btn-primary btn-sm"
            disabled={disabled}
            onClick={onSave}
          >
            {isSaving
              ? "Saving..."
              : editing
              ? isLabel
                ? "Save Label"
                : "Save Tag"
              : isLabel
              ? "Create Label"
              : "Create Tag"}
          </button>
        </div>
      </div>
      <div className="modal-backdrop" onClick={onCancel} />
    </div>
  );
};

export default ProjectTagEditModal;
