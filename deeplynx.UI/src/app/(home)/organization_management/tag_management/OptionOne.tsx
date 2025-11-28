// src/app/(home)/organization_management/tag_management/TagManagementClient.tsx
"use client";

import React, { useState } from "react";
import {
  LockClosedIcon,
  LockOpenIcon,
  PlusIcon,
} from "@heroicons/react/24/outline";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

type OrgSecurityLabel = {
  id: number;
  name: string;
  description?: string;
};

type OrgTag = {
  id: number;
  name: string;
  description?: string;
};

type TagManagementClientProps = {
  // In the future you can hydrate from the server:
  // initialLabels?: OrgSecurityLabel[];
  // initialTags?: OrgTag[];
};

/* -------------------------------------------------------------------------- */
/*                           TagManagementClient                              */
/* -------------------------------------------------------------------------- */

const OptionOne: React.FC<TagManagementClientProps> = () => {
  /* ------------------------------------------------------------------------ */
  /*                               Mocked State                               */
  /* ------------------------------------------------------------------------ */

  // TODO: Replace with server-provided initial data
  const [labels, setLabels] = useState<OrgSecurityLabel[]>([
    { id: 1, name: "CUI", description: "Controlled Unclassified Information" },
    { id: 2, name: "FOUO", description: "For Official Use Only" },
    { id: 3, name: "Internal", description: "Internal-only access" },
  ]);

  const [tags, setTags] = useState<OrgTag[]>([
    { id: 1, name: "PII", description: "Personally Identifiable Information" },
    { id: 2, name: "QA", description: "Quality assurance" },
    { id: 3, name: "Archive", description: "Legacy or archived data" },
  ]);

  const [labelsLocked, setLabelsLocked] = useState<boolean>(false);
  const [tagsLocked, setTagsLocked] = useState<boolean>(false);

  /* ------------------------------------------------------------------------ */
  /*                              Modal State                                 */
  /* ------------------------------------------------------------------------ */

  const [showLabelModal, setShowLabelModal] = useState(false);
  const [showTagModal, setShowTagModal] = useState(false);

  const [editingLabel, setEditingLabel] = useState<OrgSecurityLabel | null>(
    null
  );
  const [editingTag, setEditingTag] = useState<OrgTag | null>(null);

  const [nameInput, setNameInput] = useState("");
  const [descriptionInput, setDescriptionInput] = useState("");

  const resetModalState = () => {
    setNameInput("");
    setDescriptionInput("");
    setEditingLabel(null);
    setEditingTag(null);
  };

  /* ------------------------------------------------------------------------ */
  /*                          Handlers: Security Labels                       */
  /* ------------------------------------------------------------------------ */

  const openCreateLabelModal = () => {
    resetModalState();
    setShowLabelModal(true);
  };

  const openEditLabelModal = (label: OrgSecurityLabel) => {
    setEditingLabel(label);
    setNameInput(label.name);
    setDescriptionInput(label.description ?? "");
    setShowLabelModal(true);
  };

  const handleSaveLabel = () => {
    if (!nameInput.trim()) return;

    if (editingLabel) {
      setLabels((prev) =>
        prev.map((l) =>
          l.id === editingLabel.id
            ? { ...l, name: nameInput.trim(), description: descriptionInput }
            : l
        )
      );
    } else {
      const newId =
        labels.length > 0 ? Math.max(...labels.map((l) => l.id)) + 1 : 1;
      setLabels((prev) => [
        ...prev,
        { id: newId, name: nameInput.trim(), description: descriptionInput },
      ]);
    }

    setShowLabelModal(false);
    resetModalState();
  };

  const handleDeleteLabel = (id: number) => {
    // In a real app, confirm + call API
    setLabels((prev) => prev.filter((l) => l.id !== id));
  };

  /* ------------------------------------------------------------------------ */
  /*                               Handlers: Tags                             */
  /* ------------------------------------------------------------------------ */

  const openCreateTagModal = () => {
    resetModalState();
    setShowTagModal(true);
  };

  const openEditTagModal = (tag: OrgTag) => {
    setEditingTag(tag);
    setNameInput(tag.name);
    setDescriptionInput(tag.description ?? "");
    setShowTagModal(true);
  };

  const handleSaveTag = () => {
    if (!nameInput.trim()) return;

    if (editingTag) {
      setTags((prev) =>
        prev.map((t) =>
          t.id === editingTag.id
            ? { ...t, name: nameInput.trim(), description: descriptionInput }
            : t
        )
      );
    } else {
      const newId =
        tags.length > 0 ? Math.max(...tags.map((t) => t.id)) + 1 : 1;
      setTags((prev) => [
        ...prev,
        { id: newId, name: nameInput.trim(), description: descriptionInput },
      ]);
    }

    setShowTagModal(false);
    resetModalState();
  };

  const handleDeleteTag = (id: number) => {
    // In a real app, confirm + call API
    setTags((prev) => prev.filter((t) => t.id !== id));
  };

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      {/* Page header */}
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-base-content">
          Tag &amp; Security Label Management
        </h2>
        <p className="text-base-content/70 mt-1 max-w-2xl text-sm">
          Define organization-level tags and security labels that propagate to
          all projects. Locked items cannot be modified or extended at the
          project level.
        </p>
      </div>

      {/* Two main panels side-by-side */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* --------------------- Security Labels Panel ---------------------- */}
        <div className="card bg-base-100 border border-primary/60 shadow-md">
          <div className="card-body">
            <div className="flex items-center justify-between mb-2">
              <div>
                <h3 className="text-lg font-semibold flex items-center gap-2">
                  Security Labels
                </h3>
                <p className="text-xs text-base-content/70 mt-1">
                  These labels are used for attribute-based access control and
                  apply to every project in this organization.
                </p>
              </div>

              {/* Lock toggle */}
              <div className="flex flex-col items-end gap-1">
                <span className="text-xs text-base-content/70">
                  Project-Level Security Labels
                </span>
                <button
                  type="button"
                  className={`btn btn-xs gap-1 ${
                    labelsLocked ? "btn-error" : "btn-ghost"
                  }`}
                  onClick={() => setLabelsLocked((prev) => !prev)}
                >
                  {labelsLocked ? (
                    <>
                      <LockClosedIcon className="w-4 h-4" />
                      Locked
                    </>
                  ) : (
                    <>
                      <LockOpenIcon className="w-4 h-4" />
                      Unlocked
                    </>
                  )}
                </button>
                <span className="text-[10px] text-base-content/60 text-right">
                  {labelsLocked
                    ? "Projects cannot create security labels."
                    : "Projects may define additional security labels."}
                </span>
              </div>
            </div>

            {/* Labels table */}
            <div className="overflow-x-auto mt-4 border border-base-300 rounded-lg">
              <table className="table table-sm">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Description</th>
                    <th className="w-24 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {labels.length === 0 ? (
                    <tr>
                      <td
                        colSpan={3}
                        className="py-6 text-center text-base-content/60 text-sm"
                      >
                        No security labels defined. Create your first label.
                      </td>
                    </tr>
                  ) : (
                    labels.map((label) => (
                      <tr key={label.id} className="hover">
                        <td className="font-medium">{label.name}</td>
                        <td className="text-xs text-base-content/70">
                          {label.description || "—"}
                        </td>
                        <td className="text-right">
                          <div className="flex justify-end gap-1">
                            <button
                              type="button"
                              className="btn btn-ghost btn-xs"
                              onClick={() => openEditLabelModal(label)}
                              disabled={labelsLocked}
                              title={
                                labelsLocked
                                  ? "Labels are locked at the org level"
                                  : "Edit label"
                              }
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              className="btn btn-ghost btn-xs text-error"
                              onClick={() => handleDeleteLabel(label.id)}
                              disabled={labelsLocked}
                              title={
                                labelsLocked
                                  ? "Labels are locked at the org level"
                                  : "Remove label"
                              }
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            {/* Add label button */}
            <div className="mt-4 flex justify-end">
              <button
                type="button"
                className="btn btn-primary btn-sm gap-2"
                onClick={openCreateLabelModal}
                disabled={labelsLocked}
                title={
                  labelsLocked
                    ? "Labels are locked at the org level"
                    : "Add a new security label"
                }
              >
                <PlusIcon className="w-4 h-4" />
                New Security Label
              </button>
            </div>
          </div>
        </div>

        {/* -------------------------- Tags Panel ---------------------------- */}
        <div className="card bg-base-100 border border-primary/60 shadow-md">
          <div className="card-body">
            <div className="flex items-center justify-between mb-2">
              <div>
                <h3 className="text-lg font-semibold">Organization Tags</h3>
                <p className="text-xs text-base-content/70 mt-1">
                  Tags defined here are available across all projects and cannot
                  be modified at the project level when locked.
                </p>
              </div>

              {/* Lock toggle */}
              <div className="flex flex-col items-end gap-1">
                <span className="text-xs text-base-content/70">
                  Project-Level Tags
                </span>
                <button
                  type="button"
                  className={`btn btn-xs gap-1 ${
                    tagsLocked ? "btn-error" : "btn-ghost"
                  }`}
                  onClick={() => setTagsLocked((prev) => !prev)}
                >
                  {tagsLocked ? (
                    <>
                      <LockClosedIcon className="w-4 h-4" />
                      Locked
                    </>
                  ) : (
                    <>
                      <LockOpenIcon className="w-4 h-4" />
                      Unlocked
                    </>
                  )}
                </button>
                <span className="text-[10px] text-base-content/60 text-right">
                  {tagsLocked
                    ? "Projects cannot create new tags."
                    : "Projects may define additional tags."}
                </span>
              </div>
            </div>

            {/* Tags table */}
            <div className="overflow-x-auto mt-4 border border-base-300 rounded-lg">
              <table className="table table-sm">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Description</th>
                    <th className="w-24 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {tags.length === 0 ? (
                    <tr>
                      <td
                        colSpan={3}
                        className="py-6 text-center text-base-content/60 text-sm"
                      >
                        No tags defined. Create your first tag.
                      </td>
                    </tr>
                  ) : (
                    tags.map((tag) => (
                      <tr key={tag.id} className="hover">
                        <td className="font-medium">{tag.name}</td>
                        <td className="text-xs text-base-content/70">
                          {tag.description || "—"}
                        </td>
                        <td className="text-right">
                          <div className="flex justify-end gap-1">
                            <button
                              type="button"
                              className="btn btn-ghost btn-xs"
                              onClick={() => openEditTagModal(tag)}
                              disabled={tagsLocked}
                              title={
                                tagsLocked
                                  ? "Tags are locked at the org level"
                                  : "Edit tag"
                              }
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              className="btn btn-ghost btn-xs text-error"
                              onClick={() => handleDeleteTag(tag.id)}
                              disabled={tagsLocked}
                              title={
                                tagsLocked
                                  ? "Tags are locked at the org level"
                                  : "Remove tag"
                              }
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            {/* Add tag button */}
            <div className="mt-4 flex justify-end">
              <button
                type="button"
                className="btn btn-primary btn-sm gap-2"
                onClick={openCreateTagModal}
                disabled={tagsLocked}
                title={
                  tagsLocked
                    ? "Tags are locked at the org level"
                    : "Add a new tag"
                }
              >
                <PlusIcon className="w-4 h-4" />
                New Tag
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* ---------------------- Security Label Modal ------------------------ */}
      {showLabelModal && (
        <div className="modal modal-open">
          <div className="modal-box max-w-md">
            <h3 className="font-bold text-lg mb-2">
              {editingLabel ? "Edit Security Label" : "Create Security Label"}
            </h3>
            <p className="text-xs text-base-content/70 mb-4">
              Define or update an organization-level security label.
            </p>

            <div className="space-y-4">
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    Label Name <span className="text-error">*</span>
                  </span>
                </label>
                <input
                  type="text"
                  className="input input-bordered input-sm"
                  placeholder="e.g., CUI, FOUO, Internal"
                  value={nameInput}
                  onChange={(e) => setNameInput(e.target.value)}
                />
              </div>

              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">Description</span>
                </label>
                <textarea
                  className="textarea textarea-bordered textarea-sm"
                  placeholder="Short description of what this label means"
                  value={descriptionInput}
                  onChange={(e) => setDescriptionInput(e.target.value)}
                />
              </div>
            </div>

            <div className="modal-action">
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                onClick={() => {
                  setShowLabelModal(false);
                  resetModalState();
                }}
              >
                Cancel
              </button>
              <button
                type="button"
                className="btn btn-primary btn-sm"
                disabled={!nameInput.trim()}
                onClick={handleSaveLabel}
              >
                {editingLabel ? "Save Changes" : "Create Label"}
              </button>
            </div>
          </div>
          <div
            className="modal-backdrop"
            onClick={() => {
              setShowLabelModal(false);
              resetModalState();
            }}
          />
        </div>
      )}

      {/* --------------------------- Tag Modal ----------------------------- */}
      {showTagModal && (
        <div className="modal modal-open">
          <div className="modal-box max-w-md">
            <h3 className="font-bold text-lg mb-2">
              {editingTag ? "Edit Tag" : "Create Tag"}
            </h3>
            <p className="text-xs text-base-content/70 mb-4">
              Define or update an organization-level tag.
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
                  onChange={(e) => setNameInput(e.target.value)}
                />
              </div>

              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">Description</span>
                </label>
                <textarea
                  className="textarea textarea-bordered textarea-sm"
                  placeholder="Short description of how this tag is used"
                  value={descriptionInput}
                  onChange={(e) => setDescriptionInput(e.target.value)}
                />
              </div>
            </div>

            <div className="modal-action">
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                onClick={() => {
                  setShowTagModal(false);
                  resetModalState();
                }}
              >
                Cancel
              </button>
              <button
                type="button"
                className="btn btn-primary btn-sm"
                disabled={!nameInput.trim()}
                onClick={handleSaveTag}
              >
                {editingTag ? "Save Changes" : "Create Tag"}
              </button>
            </div>
          </div>
          <div
            className="modal-backdrop"
            onClick={() => {
              setShowTagModal(false);
              resetModalState();
            }}
          />
        </div>
      )}
    </div>
  );
};

export default OptionOne;
