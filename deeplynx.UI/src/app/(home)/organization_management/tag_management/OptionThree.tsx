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
import {
  getAllOrganizationTags,
  createOrganizationTag,
  archiveOrganizationTag,
  updateOrganizationTag,
} from "@/app/lib/client_service/tag_services.client";
import type {
  ProjectResponseDto,
  TagResponseDto,
} from "@/app/(home)/types/responseDTOs";
import ConfirmArchiveTagModal from "./ConfirmArchiveTagModal";
interface Props {
  projects: ProjectResponseDto[];
}
/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */
type ModalMode = "tag";

/* -------------------------------------------------------------------------- */
/*                       TagManagementClientOption3                           */
/* -------------------------------------------------------------------------- */

const TagManagementClientOption3 = ({ projects }: Props) => {
  /* ------------------------------------------------------------------------ */
  /*                         Organization / Core State                        */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();
  const orgId = organization?.organizationId as number | undefined;

  // 🔒 Labels are not supported yet – we only use this for display.
  const [labelsLocked] = useState(false);
  const labelCount = 0;

  // Tags loaded from backend
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [tagsLocked, setTagsLocked] = useState(false);

  const [tagsLoading, setTagsLoading] = useState(false);
  const [tagsError, setTagsError] = useState<string | null>(null);

  // For delete/archiving UX
  const [archivingTagId, setArchivingTagId] = useState<number | null>(null);

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
  const [savingTag, setSavingTag] = useState(false);
  const [showArchiveModal, setShowArchiveModal] = useState(false);
  const [tagToArchive, setTagToArchive] = useState<TagResponseDto | null>(null);

  const resetModalState = () => {
    setEditingTag(null);
    setNameInput("");
    setSavingTag(false);
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

  const openArchiveModal = (tag: TagResponseDto) => {
    setTagToArchive(tag);
    setShowArchiveModal(true);
  };

  /* ------------------------------------------------------------------------ */
  /*                           Load Tags from Backend                         */
  /* ------------------------------------------------------------------------ */

  const loadOrganizationTags = async () => {
    if (!orgId) return;

    try {
      setTagsLoading(true);
      setTagsError(null);

      const dtoList: TagResponseDto[] = await getAllOrganizationTags(
        orgId,
        undefined,
        true // hide archived by default
      );

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
  }, [orgId]);

  /* ------------------------------------------------------------------------ */
  /*                          Save / Delete Handlers                          */
  /* ------------------------------------------------------------------------ */

  const handleSave = async () => {
    if (!nameInput.trim()) return;
    if (!orgId) {
      toast.error("No organization selected. Unable to save tag.");
      return;
    }

    if (modalMode !== "tag") return;

    try {
      setSavingTag(true);

      if (editingTag) {
        const updatePayload: TagResponseDto = {
          ...editingTag,
          name: nameInput.trim(),
        };

        const updated = await updateOrganizationTag(
          orgId,
          editingTag.id,
          updatePayload
        );

        // Use the server's response to update local state
        setTags((prev) => prev.map((t) => (t.id === updated.id ? updated : t)));

        toast.success("Organization tag updated.");
      } else {
        const createPayload: TagResponseDto = {
          id: 0, // backend should ignore / overwrite
          name: nameInput.trim(),
          projectId: 0, // sentinel for "org-level" if that's how your API works
          isArchived: false,
          lastUpdatedAt: null,
          lastUpdatedBy: null,
          archivedAt: null,
        };

        const created = await createOrganizationTag(orgId, createPayload);
        setTags((prev) => [...prev, created]);
        toast.success("Organization tag created.");
      }

      closeModal();
    } catch (error) {
      console.error("Failed to save organization tag:", error);
      toast.error("Failed to save organization tag.");
    } finally {
      setSavingTag(false);
    }
  };

  const confirmArchiveTag = async () => {
    if (!tagToArchive || !orgId) return;

    try {
      setArchivingTagId(tagToArchive.id);
      await archiveOrganizationTag(orgId, tagToArchive.id, true);

      setTags((prev) => prev.filter((t) => t.id !== tagToArchive.id));
      toast.success(`Tag "${tagToArchive.name}" archived.`);
    } catch (error) {
      console.error("Failed to archive tag:", error);
      toast.error("Failed to archive tag.");
    } finally {
      setArchivingTagId(null);
      setShowArchiveModal(false);
      setTagToArchive(null);
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                               Derived Data                               */
  /* ------------------------------------------------------------------------ */

  const tagCount = tags.length;

  // Mock “affected project counts” – adjust/remove once wired to backend
  const projectsWithLabels = 0; // labels not yet supported
  const projectsWithTags = projects.length;

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
                  disabled={tagsLocked || !orgId}
                  title={
                    !orgId
                      ? "No organization selected"
                      : tagsLocked
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
                        onClick={() => openArchiveModal(tag)}
                        disabled={tagsLocked || archivingTagId === tag.id}
                        title={
                          tagsLocked
                            ? "Tags are locked"
                            : "Archive (soft delete) tag"
                        }
                      >
                        {archivingTagId === tag.id ? "Archiving..." : "Delete"}
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
                disabled={!nameInput.trim() || savingTag}
                onClick={handleSave}
              >
                {savingTag
                  ? "Saving..."
                  : editingTag
                  ? "Save Tag"
                  : "Create Tag"}
              </button>
            </div>
          </div>
          <div className="modal-backdrop" onClick={closeModal} />
        </div>
      )}

      <ConfirmArchiveTagModal
        isOpen={showArchiveModal}
        tagName={tagToArchive?.name ?? ""}
        onClose={() => {
          setShowArchiveModal(false);
          setTagToArchive(null);
        }}
        onConfirm={confirmArchiveTag}
        loading={archivingTagId === tagToArchive?.id}
      />
    </div>
  );
};

export default TagManagementClientOption3;
