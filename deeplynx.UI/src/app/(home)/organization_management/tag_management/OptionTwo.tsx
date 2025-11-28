// src/app/(home)/organization_management/tag_management/TagManagementClientOption2.tsx
"use client";

import React, { useState } from "react";
import {
  LockClosedIcon,
  LockOpenIcon,
  PlusIcon,
  TagIcon,
  ShieldCheckIcon,
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

type ActiveTab = "labels" | "tags";

/* -------------------------------------------------------------------------- */
/*                         TagManagementClientOption2                         */
/* -------------------------------------------------------------------------- */

const OptionTwo: React.FC = () => {
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
  /*                               View State                                 */
  /* ------------------------------------------------------------------------ */

  const [activeTab, setActiveTab] = useState<ActiveTab>("labels");

  /* ------------------------------------------------------------------------ */
  /*                              Modal State                                 */
  /* ------------------------------------------------------------------------ */

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<ActiveTab>("labels");
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

  const openCreateModal = (mode: ActiveTab) => {
    resetModalState();
    setModalMode(mode);
    setIsModalOpen(true);
  };

  const openEditModal = (mode: ActiveTab, id: number) => {
    setModalMode(mode);

    if (mode === "labels") {
      const found = labels.find((l) => l.id === id) || null;
      if (found) {
        setEditingLabel(found);
        setNameInput(found.name);
        setDescriptionInput(found.description ?? "");
      }
    } else {
      const found = tags.find((t) => t.id === id) || null;
      if (found) {
        setEditingTag(found);
        setNameInput(found.name);
        setDescriptionInput(found.description ?? "");
      }
    }

    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    resetModalState();
  };

  /* ------------------------------------------------------------------------ */
  /*                          Save / Delete Handlers                          */
  /* ------------------------------------------------------------------------ */

  const handleSave = () => {
    if (!nameInput.trim()) return;

    if (modalMode === "labels") {
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
          {
            id: newId,
            name: nameInput.trim(),
            description: descriptionInput,
          },
        ]);
      }
    } else {
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
          {
            id: newId,
            name: nameInput.trim(),
            description: descriptionInput,
          },
        ]);
      }
    }

    closeModal();
  };

  const handleDeleteLabel = (id: number) => {
    setLabels((prev) => prev.filter((l) => l.id !== id));
  };

  const handleDeleteTag = (id: number) => {
    setTags((prev) => prev.filter((t) => t.id !== id));
  };

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  const labelsLockedText = labelsLocked ? "Locked" : "Unlocked";
  const tagsLockedText = tagsLocked ? "Locked" : "Unlocked";

  return (
    <div className="p-6">
      {/* Page header */}
      <div className="mb-4">
        <h2 className="text-2xl font-bold text-base-content">
          Tag &amp; Security Label Management
        </h2>
        <p className="text-base-content/70 mt-1 max-w-2xl text-sm">
          Define organization-wide security labels and tags. You can lock each
          type to prevent projects from creating their own labels or tags.
        </p>
      </div>

      {/* Global summary banner */}
      <div className="mb-6">
        <div className="alert bg-base-200 border border-base-300 flex items-center justify-between">
          <div className="flex flex-col sm:flex-row sm:items-center gap-2">
            <div className="font-semibold text-sm">
              Organization-wide lock status
            </div>
            <div className="flex flex-wrap gap-2 text-xs">
              <span className="badge badge-outline gap-1">
                <ShieldCheckIcon className="w-3 h-3" />
                Security Labels: {labelsLockedText}
              </span>
              <span className="badge badge-outline gap-1">
                <TagIcon className="w-3 h-3" />
                Tags: {tagsLockedText}
              </span>
            </div>
          </div>
          <div className="text-[10px] text-base-content/60 text-right max-w-xs">
            Locked items cannot be created or edited at the project level. Only
            organization administrators can modify them here.
          </div>
        </div>
      </div>

      {/* Card with tabs */}
      <div className="card bg-base-100 border border-primary/70 shadow-md">
        <div className="card-body">
          {/* Tabs */}
          <div className="flex items-center justify-between border-b border-base-300 pb-2 mb-4">
            <div className="tabs tabs-bordered">
              <button
                type="button"
                className={`tab tab-sm ${
                  activeTab === "labels" ? "tab-active" : ""
                }`}
                onClick={() => setActiveTab("labels")}
              >
                Security Labels
              </button>
              <button
                type="button"
                className={`tab tab-sm ${
                  activeTab === "tags" ? "tab-active" : ""
                }`}
                onClick={() => setActiveTab("tags")}
              >
                Tags
              </button>
            </div>

            {/* Contextual action button */}
            <div className="flex items-center gap-2">
              {activeTab === "labels" ? (
                <>
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
                  <button
                    type="button"
                    className="btn btn-primary btn-sm gap-2"
                    onClick={() => openCreateModal("labels")}
                    disabled={labelsLocked}
                    title={
                      labelsLocked
                        ? "Security labels are locked at the org level"
                        : "Create a new security label"
                    }
                  >
                    <PlusIcon className="w-4 h-4" />
                    New Label
                  </button>
                </>
              ) : (
                <>
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
                  <button
                    type="button"
                    className="btn btn-primary btn-sm gap-2"
                    onClick={() => openCreateModal("tags")}
                    disabled={tagsLocked}
                    title={
                      tagsLocked
                        ? "Tags are locked at the org level"
                        : "Create a new tag"
                    }
                  >
                    <PlusIcon className="w-4 h-4" />
                    New Tag
                  </button>
                </>
              )}
            </div>
          </div>

          {/* Tab content */}
          {activeTab === "labels" ? (
            <div>
              <p className="text-xs text-base-content/70 mb-3">
                Organization-level security labels are used for attribute-based
                access control (ABAC) and are inherited by all projects.
              </p>
              <div className="overflow-x-auto border border-base-300 rounded-lg">
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
                          className="py-6 text-center text-sm text-base-content/60"
                        >
                          No security labels defined yet.
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
                                onClick={() =>
                                  openEditModal("labels", label.id)
                                }
                                disabled={labelsLocked}
                                title={
                                  labelsLocked
                                    ? "Security labels are locked"
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
                                    ? "Security labels are locked"
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
            </div>
          ) : (
            <div>
              <p className="text-xs text-base-content/70 mb-3">
                Organization-level tags are available to all projects. When
                locked, projects cannot create their own tags.
              </p>
              <div className="overflow-x-auto border border-base-300 rounded-lg">
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
                          className="py-6 text-center text-sm text-base-content/60"
                        >
                          No tags defined yet.
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
                                onClick={() => openEditModal("tags", tag.id)}
                                disabled={tagsLocked}
                                title={
                                  tagsLocked ? "Tags are locked" : "Edit tag"
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
                                  tagsLocked ? "Tags are locked" : "Remove tag"
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
            </div>
          )}
        </div>
      </div>

      {/* ------------------------- Shared Modal ----------------------------- */}
      {isModalOpen && (
        <div className="modal modal-open">
          <div className="modal-box max-w-md">
            <h3 className="font-bold text-lg mb-2">
              {modalMode === "labels"
                ? editingLabel
                  ? "Edit Security Label"
                  : "Create Security Label"
                : editingTag
                ? "Edit Tag"
                : "Create Tag"}
            </h3>
            <p className="text-xs text-base-content/70 mb-4">
              {modalMode === "labels"
                ? "Define or update an organization-level security label. These are used for access control and are visible to all projects."
                : "Define or update an organization-level tag. These tags propagate to every project in this organization."}
            </p>

            <div className="space-y-4">
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    {modalMode === "labels" ? "Label Name" : "Tag Name"}{" "}
                    <span className="text-error">*</span>
                  </span>
                </label>
                <input
                  type="text"
                  className="input input-bordered input-sm"
                  placeholder={
                    modalMode === "labels"
                      ? "e.g., CUI, FOUO, Internal"
                      : "e.g., PII, QA, Archive"
                  }
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
                  placeholder={
                    modalMode === "labels"
                      ? "Short description of what this label means"
                      : "Short description of how this tag is used"
                  }
                  value={descriptionInput}
                  onChange={(e) => setDescriptionInput(e.target.value)}
                />
              </div>
            </div>

            <div className="modal-action">
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                onClick={closeModal}
              >
                Cancel
              </button>
              <button
                type="button"
                className="btn btn-primary btn-sm"
                disabled={!nameInput.trim()}
                onClick={handleSave}
              >
                {modalMode === "labels"
                  ? editingLabel
                    ? "Save Label"
                    : "Create Label"
                  : editingTag
                  ? "Save Tag"
                  : "Create Tag"}
              </button>
            </div>
          </div>
          <div className="modal-backdrop" onClick={closeModal} />
        </div>
      )}
    </div>
  );
};

export default OptionTwo;
