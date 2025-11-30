// src/app/(home)/organization_management/tag_management/TagManagementClientOption3.tsx
"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  LockClosedIcon,
  LockOpenIcon,
  ShieldCheckIcon,
  TagIcon,
  PlusIcon,
  InformationCircleIcon,
  MagnifyingGlassIcon,
} from "@heroicons/react/24/outline";
import toast from "react-hot-toast";

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { getAllOrganizationTags } from "@/app/lib/client_service/tag_services.client";
import type { TagResponseDto } from "@/app/(home)/types/responseDTOs";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */
type ModalMode = "tag";

/* -------------------------------------------------------------------------- */
/*                       TagManagementClientOption3                           */
/* -------------------------------------------------------------------------- */

const TagManagementClientOption3: React.FC = () => {
  /* ------------------------------------------------------------------------ */
  /*                         Organization / Core State                        */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();

  // 🔒 Labels are not supported yet – we only use this for display.
  const [labelsLocked] = useState(false);
  const labelCount = 0;

  // Tags loaded from backend
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [tagsLocked, setTagsLocked] = useState(false);

  const [tagsLoading, setTagsLoading] = useState(false);
  const [tagsError, setTagsError] = useState<string | null>(null);

  /* ------------------------------------------------------------------------ */
  /*                               Search State                               */
  /* ------------------------------------------------------------------------ */

  const [tagSearch, setTagSearch] = useState("");

  const normalizedTagSearch = tagSearch.trim().toLowerCase();

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
  const [modalMode, setModalMode] = useState<ModalMode>("tag");
  const [editingTag, setEditingTag] = useState<TagResponseDto | null>(null);
  const [nameInput, setNameInput] = useState("");

  const resetModalState = () => {
    setEditingTag(null);
    setNameInput("");
  };

  const openCreateTagModal = () => {
    resetModalState();
    setModalMode("tag");
    setIsModalOpen(true);
  };

  const openEditTagModal = (id: number) => {
    resetModalState();
    setModalMode("tag");
    const found = tags.find((t) => t.id === id) || null;
    if (found) {
      setEditingTag(found);
      setNameInput(found.name);
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    resetModalState();
  };

  /* ------------------------------------------------------------------------ */
  /*                           Load Tags from Backend                         */
  /* ------------------------------------------------------------------------ */

  const loadOrganizationTags = async () => {
    if (!organization?.organizationId) return;

    try {
      setTagsLoading(true);
      setTagsError(null);

      const dtoList: TagResponseDto[] = await getAllOrganizationTags(
        organization.organizationId as number,
        undefined,
        true // hide archived by default
      );

      // Map DTO → local OrgTag
      // Keep full DTOs
      setTags(dtoList.filter((t) => !t.isArchived));
    } catch (error) {
      console.error("Failed to load organization tags:", error);
      setTagsError("Failed to load organization tags.");
      toast.error("Failed to load organization tags.");
    } finally {
      setTagsLoading(false);
    }
  };

  useEffect(() => {
    loadOrganizationTags();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [organization?.organizationId]);

  /* ------------------------------------------------------------------------ */
  /*                          Save / Delete Handlers                          */
  /* ------------------------------------------------------------------------ */

  // NOTE: For now these update *local state only*.
  // When you wire create/update/delete APIs, call them here and then
  // either refetch tags or update state based on the response.

  const handleSave = () => {
    if (!nameInput.trim()) return;

    if (modalMode === "tag") {
      if (editingTag) {
        // editing existing tag
        setTags((prev) =>
          prev.map((t) =>
            t.id === editingTag.id
              ? {
                  ...t,
                  name: nameInput.trim(),
                  // optional: update metadata if you want
                  lastUpdatedAt: new Date().toISOString(),
                }
              : t
          )
        );
      } else {
        // creating new tag (local only for now)
        const newId =
          tags.length > 0 ? Math.max(...tags.map((t) => t.id)) + 1 : 1;

        const newTag: TagResponseDto = {
          id: newId,
          name: nameInput.trim(),
          projectId: 0, // or whatever your "org-level tag" sentinel is
          isArchived: false,
          lastUpdatedAt: null,
          lastUpdatedBy: null,
          archivedAt: null,
        };

        setTags((prev) => [...prev, newTag]);
      }
    }

    // labels branch unchanged…
    // (or similarly updated if you moved them to their own DTO)

    closeModal();
  };

  const handleDeleteTag = (id: number) => {
    setTags((prev) => prev.filter((t) => t.id !== id));
  };

  /* ------------------------------------------------------------------------ */
  /*                               Derived Data                               */
  /* ------------------------------------------------------------------------ */

  const tagCount = tags.length;

  // Mock “affected project counts” – adjust/remove once wired to backend
  const projectsWithLabels = 0; // labels not yet supported
  const projectsWithTags = Math.max(1, Math.min(12, tagCount * 2));

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      {/* Page Header */}
      <div className="mb-4">
        <h2 className="text-2xl font-bold text-base-content">Tag Management</h2>
        <p className="text-base-content/70 mt-1 max-w-3xl text-sm">
          Define organization-wide tags today. Security labels will be added in
          a future release and will appear here once available.
        </p>
      </div>

      {/* Policy Overview Strip */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-3 mb-6">
        {/* Security Labels – Coming Soon */}
        <div className="stat bg-base-100 border border-dashed border-base-300 rounded-xl opacity-60">
          <div className="stat-title flex items-center gap-1 text-xs">
            <ShieldCheckIcon className="w-4 h-4 text-base-content/50" />
            Org Security Labels
          </div>
          <div className="stat-value text-base-content/60 text-xl">
            {labelCount}
          </div>
          <div className="stat-desc text-xs flex items-center gap-1 text-base-content/60">
            <InformationCircleIcon className="w-4 h-4" />
            <span>Security labels coming soon</span>
          </div>
        </div>

        {/* Projects with Labels – Coming Soon */}
        <div className="stat bg-base-100 border border-dashed border-base-300 rounded-xl opacity-60">
          <div className="stat-title flex items-center gap-1 text-xs">
            <ShieldCheckIcon className="w-4 h-4 text-base-content/50" />
            Projects with Labels
          </div>
          <div className="stat-value text-base-content/60 text-xl">
            {projectsWithLabels}
          </div>
          <div className="stat-desc text-xs text-base-content/60">
            Will reflect usage once labels are enabled
          </div>
        </div>

        {/* Tags */}
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
        {/* Security Labels Card – Coming Soon (visually disabled) */}
        <div className="card bg-base-100 border border-dashed border-base-300 shadow-sm opacity-60">
          <div className="card-body">
            <div className="flex items-start justify-between gap-4 mb-3">
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <ShieldCheckIcon className="w-5 h-5 text-base-content/60" />
                  <h3 className="font-semibold text-base text-base-content/80">
                    Organization Security Labels
                  </h3>
                  <span className="badge badge-sm badge-ghost">
                    Coming soon
                  </span>
                </div>
                <p className="text-xs text-base-content/70 mt-1 max-w-md">
                  Security labels (e.g., CUI) for attribute-based access control
                  will appear here in a future release. Projects will inherit
                  labels defined at the organization level.
                </p>
              </div>
            </div>

            <div className="flex items-start gap-2 text-xs text-base-content/70 mt-2">
              <InformationCircleIcon className="w-4 h-4" />
              <p>
                You&apos;ll be able to define and lock organization-wide
                security labels here. Until then, only tags are available for
                project use.
              </p>
            </div>

            <div className="mt-4 py-4 text-center text-xs text-base-content/60 border border-dashed border-base-300 rounded-lg">
              Security label management is not yet enabled for this
              organization.
            </div>
          </div>
        </div>

        {/* Tags Card – Fully Functional */}
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
                  onClick={openCreateTagModal}
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
              {tagsLoading ? (
                <div className="py-6 text-center text-xs text-base-content/60">
                  Loading organization tags…
                </div>
              ) : tagsError ? (
                <div className="py-6 text-center text-xs text-error">
                  {tagsError}
                </div>
              ) : filteredTags.length === 0 ? (
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
                        onClick={() => openEditTagModal(tag.id)}
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

      {/* Tag Modal Only (labels not yet supported) */}
      {isModalOpen && modalMode === "tag" && (
        <div className="modal modal-open">
          <div className="modal-box max-w-md">
            <h3 className="font-bold text-lg mb-2">
              {editingTag ? "Edit Tag" : "Create Tag"}
            </h3>
            <p className="text-xs text-base-content/70 mb-4">
              Define an organization-level tag. Projects inherit this tag and
              can use it across their assets.
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
                {editingTag ? "Save Tag" : "Create Tag"}
              </button>
            </div>
          </div>
          <div className="modal-backdrop" onClick={closeModal} />
        </div>
      )}
    </div>
  );
};

export default TagManagementClientOption3;
