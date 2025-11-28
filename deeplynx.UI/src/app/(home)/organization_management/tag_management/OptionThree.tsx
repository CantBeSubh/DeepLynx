// src/app/(home)/organization_management/tag_management/TagManagementClientOption3.tsx
"use client";

import React, { useMemo, useState } from "react";
import {
  LockClosedIcon,
  LockOpenIcon,
  ShieldCheckIcon,
  TagIcon,
  PlusIcon,
  InformationCircleIcon,
  MagnifyingGlassIcon,
} from "@heroicons/react/24/outline";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

type OrgSecurityLabel = {
  id: number;
  name: string;
};

type OrgTag = {
  id: number;
  name: string;
};

type ModalMode = "label" | "tag";

/* -------------------------------------------------------------------------- */
/*                       TagManagementClientOption3                           */
/* -------------------------------------------------------------------------- */

const OptionThree: React.FC = () => {
  /* ------------------------------------------------------------------------ */
  /*                               Mocked State                               */
  /* ------------------------------------------------------------------------ */

  // TODO: replace with server data
  const [labels, setLabels] = useState<OrgSecurityLabel[]>([
    { id: 1, name: "CUI" },
    { id: 2, name: "FOUO" },
    { id: 3, name: "Internal" },
  ]);

  const [tags, setTags] = useState<OrgTag[]>([
    { id: 1, name: "PII" },
    { id: 2, name: "QA" },
    { id: 3, name: "Archive" },
  ]);

  const [labelsLocked, setLabelsLocked] = useState(false);
  const [tagsLocked, setTagsLocked] = useState(false);

  /* ------------------------------------------------------------------------ */
  /*                               Search State                               */
  /* ------------------------------------------------------------------------ */

  const [labelSearch, setLabelSearch] = useState("");
  const [tagSearch, setTagSearch] = useState("");

  const normalizedLabelSearch = labelSearch.trim().toLowerCase();
  const normalizedTagSearch = tagSearch.trim().toLowerCase();

  const filteredLabels = useMemo(
    () =>
      normalizedLabelSearch
        ? labels.filter((l) =>
            l.name.toLowerCase().includes(normalizedLabelSearch)
          )
        : labels,
    [labels, normalizedLabelSearch]
  );

  const filteredTags = useMemo(
    () =>
      normalizedTagSearch
        ? tags.filter((t) => t.name.toLowerCase().includes(normalizedTagSearch))
        : tags,
    [tags, normalizedTagSearch]
  );

  /* ------------------------------------------------------------------------ */
  /*                               Modal State                                */
  /* ------------------------------------------------------------------------ */

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<ModalMode>("label");
  const [editingLabel, setEditingLabel] = useState<OrgSecurityLabel | null>(
    null
  );
  const [editingTag, setEditingTag] = useState<OrgTag | null>(null);

  const [nameInput, setNameInput] = useState("");

  const resetModalState = () => {
    setEditingLabel(null);
    setEditingTag(null);
    setNameInput("");
  };

  const openCreateModal = (mode: ModalMode) => {
    resetModalState();
    setModalMode(mode);
    setIsModalOpen(true);
  };

  const openEditModal = (mode: ModalMode, id: number) => {
    resetModalState();
    setModalMode(mode);

    if (mode === "label") {
      const found = labels.find((l) => l.id === id) || null;
      if (found) {
        setEditingLabel(found);
        setNameInput(found.name);
      }
    } else {
      const found = tags.find((t) => t.id === id) || null;
      if (found) {
        setEditingTag(found);
        setNameInput(found.name);
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

    if (modalMode === "label") {
      if (editingLabel) {
        setLabels((prev) =>
          prev.map((l) =>
            l.id === editingLabel.id ? { ...l, name: nameInput.trim() } : l
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
          },
        ]);
      }
    } else {
      if (editingTag) {
        setTags((prev) =>
          prev.map((t) =>
            t.id === editingTag.id ? { ...t, name: nameInput.trim() } : t
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
  /*                               Derived Data                               */
  /* ------------------------------------------------------------------------ */

  const labelCount = labels.length;
  const tagCount = tags.length;

  // Mock “affected project counts”
  const projectsWithLabels = Math.max(1, Math.min(12, labelCount * 3));
  const projectsWithTags = Math.max(1, Math.min(12, tagCount * 2));

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      {/* Page Header */}
      <div className="mb-4">
        <h2 className="text-2xl font-bold text-base-content">
          Tag &amp; Security Label Management
        </h2>
        <p className="text-base-content/70 mt-1 max-w-3xl text-sm">
          Establish organization-wide policies for security labels and tags.
          These definitions propagate to all projects and can optionally lock
          project-level customization.
        </p>
      </div>

      {/* Policy Overview Strip */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-3 mb-6">
        <div className="stat bg-base-100 border border-base-300 rounded-xl">
          <div className="stat-title flex items-center gap-1 text-xs">
            <ShieldCheckIcon className="w-4 h-4 text-primary" />
            Org Security Labels
          </div>
          <div className="stat-value text-primary text-xl">{labelCount}</div>
          <div className="stat-desc text-xs flex items-center gap-1">
            {labelsLocked ? (
              <>
                <LockClosedIcon className="w-4 h-4 text-error" />
                <span>Locked for all projects</span>
              </>
            ) : (
              <>
                <LockOpenIcon className="w-4 h-4 text-success" />
                <span>Projects may define their own</span>
              </>
            )}
          </div>
        </div>

        <div className="stat bg-base-100 border border-base-300 rounded-xl">
          <div className="stat-title flex items-center gap-1 text-xs">
            <ShieldCheckIcon className="w-4 h-4 text-secondary" />
            Projects with Labels
          </div>
          <div className="stat-value text-secondary text-xl">
            {projectsWithLabels}
          </div>
          <div className="stat-desc text-xs text-base-content/70">
            Using organization-level security labels
          </div>
        </div>

        <div className="stat bg-base-100 border border-base-300 rounded-xl">
          <div className="stat-title flex items-center gap-1 text-xs">
            <TagIcon className="w-4 h-4 text-primary" />
            Org Tags
          </div>
          <div className="stat-value text-primary text-xl">{tagCount}</div>
          <div className="stat-desc text-xs flex items-center gap-1">
            {tagsLocked ? (
              <>
                <LockClosedIcon className="w-4 h-4 text-error" />
                <span>Locked for all projects</span>
              </>
            ) : (
              <>
                <LockOpenIcon className="w-4 h-4 text-success" />
                <span>Projects may define their own</span>
              </>
            )}
          </div>
        </div>

        <div className="stat bg-base-100 border border-base-300 rounded-xl">
          <div className="stat-title flex items-center gap-1 text-xs">
            <TagIcon className="w-4 h-4 text-secondary" />
            Projects with Tags
          </div>
          <div className="stat-value text-secondary text-xl">
            {projectsWithTags}
          </div>
          <div className="stat-desc text-xs text-base-content/70">
            Inheriting organization-level tags
          </div>
        </div>
      </div>

      {/* Two-Column Policy Cards */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Security Labels Card */}
        <div className="card bg-base-100 border border-primary/70 shadow-sm">
          <div className="card-body">
            <div className="flex items-start justify-between gap-4 mb-3">
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <ShieldCheckIcon className="w-5 h-5 text-primary" />
                  <h3 className="font-semibold text-base">
                    Organization Security Labels
                  </h3>
                </div>
                <p className="text-xs text-base-content/70 mt-1 max-w-md">
                  Labels used for attribute-based access control (ABAC) and
                  sensitivity marking. All projects inherit these labels.
                </p>
              </div>

              <div className="flex flex-col items-end gap-2">
                {/* Lock toggle */}
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

                {/* Search compact */}
                <div className="form-control w-40">
                  <div className="input input-xs input-bordered flex items-center gap-1 px-2">
                    <MagnifyingGlassIcon className="w-3 h-3 text-base-content/60" />
                    <input
                      type="text"
                      className="grow text-[0.7rem] bg-transparent focus:outline-none"
                      placeholder="Search labels..."
                      value={labelSearch}
                      onChange={(e) => setLabelSearch(e.target.value)}
                    />
                  </div>
                </div>

                {/* Add button */}
                <button
                  type="button"
                  className="btn btn-primary btn-xs gap-1"
                  onClick={() => openCreateModal("label")}
                  disabled={labelsLocked}
                  title={
                    labelsLocked
                      ? "Security labels are locked at the org level"
                      : "Create new security label"
                  }
                >
                  <PlusIcon className="w-3 h-3" />
                  New Label
                </button>
              </div>
            </div>

            <div className="flex items-start gap-2 mb-3 text-xs text-base-content/70">
              <InformationCircleIcon className="w-4 h-4" />
              <p>
                When locked, projects{" "}
                <span className="font-semibold">cannot create or modify</span>{" "}
                security labels locally. They may only apply the labels defined
                here.
              </p>
            </div>

            {/* Labels list */}
            <div className="space-y-2 max-h-72 overflow-y-auto">
              {filteredLabels.length === 0 ? (
                <div className="py-6 text-center text-xs text-base-content/60 border border-dashed border-base-300 rounded-lg">
                  {labelSearch.trim()
                    ? "No security labels match your search."
                    : "No security labels defined. Create at least one label to enable ABAC policies."}
                </div>
              ) : (
                filteredLabels.map((label) => (
                  <div
                    key={label.id}
                    className="flex items-center justify-between bg-base-200/70 hover:bg-base-300/80 transition rounded-lg px-3 py-2"
                  >
                    <div className="flex items-center gap-2">
                      <span className="badge badge-outline badge-sm">
                        {label.name}
                      </span>
                      <span className="text-[0.7rem] text-base-content/70">
                        Used across all projects
                      </span>
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        className="btn btn-ghost btn-xs"
                        onClick={() => openEditModal("label", label.id)}
                        disabled={labelsLocked}
                        title={
                          labelsLocked ? "Security labels are locked" : "Edit"
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
                          labelsLocked ? "Security labels are locked" : "Delete"
                        }
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>

        {/* Tags Card */}
        <div className="card bg-base-100 border border-secondary/60 shadow-sm">
          <div className="card-body">
            <div className="flex items-start justify-between gap-4 mb-3">
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <TagIcon className="w-5 h-5 text-secondary" />
                  <h3 className="font-semibold text-base">Organization Tags</h3>
                </div>
                <p className="text-xs text-base-content/70 mt-1 max-w-md">
                  Tags for classification, workflows, and search. All projects
                  inherit these and can optionally add their own.
                </p>
              </div>

              <div className="flex flex-col items-end gap-2">
                {/* Lock toggle */}
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

                {/* Search compact */}
                <div className="form-control w-40">
                  <div className="input input-xs input-bordered flex items-center gap-1 px-2">
                    <MagnifyingGlassIcon className="w-3 h-3 text-base-content/60" />
                    <input
                      type="text"
                      className="grow text-[0.7rem] bg-transparent focus:outline-none"
                      placeholder="Search tags..."
                      value={tagSearch}
                      onChange={(e) => setTagSearch(e.target.value)}
                    />
                  </div>
                </div>

                {/* Add button */}
                <button
                  type="button"
                  className="btn btn-primary btn-xs gap-1"
                  onClick={() => openCreateModal("tag")}
                  disabled={tagsLocked}
                  title={
                    tagsLocked
                      ? "Tags are locked at the org level"
                      : "Create new tag"
                  }
                >
                  <PlusIcon className="w-3 h-3" />
                  New Tag
                </button>
              </div>
            </div>

            <div className="flex items-start gap-2 mb-3 text-xs text-base-content/70">
              <InformationCircleIcon className="w-4 h-4" />
              <p>
                When locked, projects{" "}
                <span className="font-semibold">cannot define new tags</span>{" "}
                and must use only the tags defined at the organization level.
              </p>
            </div>

            {/* Tags list */}
            <div className="space-y-2 max-h-72 overflow-y-auto">
              {filteredTags.length === 0 ? (
                <div className="py-6 text-center text-xs text-base-content/60 border border-dashed border-base-300 rounded-lg">
                  {tagSearch.trim()
                    ? "No tags match your search."
                    : "No tags defined. Create tags to standardize metadata across all projects."}
                </div>
              ) : (
                filteredTags.map((tag) => (
                  <div
                    key={tag.id}
                    className="flex items-center justify-between bg-base-200/70 hover:bg-base-300/80 transition rounded-lg px-3 py-2"
                  >
                    <div className="flex items-center gap-2">
                      <span className="badge badge-secondary badge-outline badge-sm">
                        {tag.name}
                      </span>
                      <span className="text-[0.7rem] text-base-content/70">
                        Inherited by all projects
                      </span>
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        className="btn btn-ghost btn-xs"
                        onClick={() => openEditModal("tag", tag.id)}
                        disabled={tagsLocked}
                        title={tagsLocked ? "Tags are locked" : "Edit"}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="btn btn-ghost btn-xs text-error"
                        onClick={() => handleDeleteTag(tag.id)}
                        disabled={tagsLocked}
                        title={tagsLocked ? "Tags are locked" : "Delete"}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Shared Modal */}
      {isModalOpen && (
        <div className="modal modal-open">
          <div className="modal-box max-w-md">
            <h3 className="font-bold text-lg mb-2">
              {modalMode === "label"
                ? editingLabel
                  ? "Edit Security Label"
                  : "Create Security Label"
                : editingTag
                ? "Edit Tag"
                : "Create Tag"}
            </h3>
            <p className="text-xs text-base-content/70 mb-4">
              {modalMode === "label"
                ? "Define an organization-level security label. Projects inherit this label and can apply it to records and files."
                : "Define an organization-level tag. Projects inherit this tag and can use it across their assets."}
            </p>

            <div className="space-y-4">
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    {modalMode === "label" ? "Label Name" : "Tag Name"}{" "}
                    <span className="text-error">*</span>
                  </span>
                </label>
                <input
                  type="text"
                  className="input input-bordered input-sm"
                  placeholder={
                    modalMode === "label"
                      ? "e.g., CUI, FOUO, Internal"
                      : "e.g., PII, QA, Archive"
                  }
                  value={nameInput}
                  onChange={(e) => setNameInput(e.target.value)}
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
                {modalMode === "label"
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

export default OptionThree;
