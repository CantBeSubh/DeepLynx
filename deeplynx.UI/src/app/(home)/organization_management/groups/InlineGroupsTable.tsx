// src/app/(home)/organization_management/groups/InlineGroupsTable.tsx

"use client";

import React, { useState } from "react";
import { GroupResponseDto, UserResponseDto } from "../../types/responseDTOs";
import {
  createGroup,
  updateGroup,
  deleteGroup,
  addMemberToGroup,
  removeMemberFromGroup,
} from "@/app/lib/group_services.client";
import {
  TrashIcon,
  PencilIcon,
  UserGroupIcon,
  XMarkIcon,
  MagnifyingGlassIcon,
  PlusIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  CheckIcon,
} from "@heroicons/react/24/outline";

interface InlineGroupsTableProps {
  initialGroups: GroupResponseDto[];
  availableUsers: UserResponseDto[];
  organizationId?: number | string;
}

const InlineGroupsTable: React.FC<InlineGroupsTableProps> = ({
  initialGroups,
  availableUsers,
  organizationId,
}) => {
  // State management
  const [groups, setGroups] = useState<GroupResponseDto[]>(initialGroups);
  const [expandedGroup, setExpandedGroup] = useState<string | number | null>(
    null
  );
  const [editingGroup, setEditingGroup] = useState<string | number | null>(
    null
  );
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Create form state
  const [newGroupName, setNewGroupName] = useState("");
  const [newGroupDescription, setNewGroupDescription] = useState("");

  // Edit form state
  const [editName, setEditName] = useState("");
  const [editDescription, setEditDescription] = useState("");

  // Selection state
  const [selectedGroups, setSelectedGroups] = useState<Set<string | number>>(
    new Set()
  );

  // Update the toggleExpand function:
  const toggleExpand = (groupId: string | number) => {
    if (expandedGroup === groupId) {
      setExpandedGroup(null);
      setEditingGroup(null);
    } else {
      setExpandedGroup(groupId);
      setEditingGroup(null);
    }
  };

  // Start editing
  const startEdit = (group: GroupResponseDto) => {
    setEditingGroup(group.id);
    setEditName(group.name);
    setEditDescription(group.description || "");
  };

  // Cancel editing
  const cancelEdit = () => {
    setEditingGroup(null);
    setEditName("");
    setEditDescription("");
  };

  // Create group
  const handleCreateGroup = async () => {
    if (!newGroupName.trim() || !organizationId) {
      setError("Group name is required");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const newGroup = await createGroup(
        organizationId,
        newGroupName,
        newGroupDescription
      );
      setGroups([...groups, newGroup]);
      setNewGroupName("");
      setNewGroupDescription("");
      setShowCreateForm(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create group");
    } finally {
      setLoading(false);
    }
  };

  // Update group
  const handleUpdateGroup = async (groupId: string | number) => {
    if (!editName.trim()) {
      setError("Group name is required");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const updatedGroup = await updateGroup(
        groupId,
        editName,
        editDescription
      );
      setGroups(groups.map((g) => (g.id === groupId ? updatedGroup : g)));
      setEditingGroup(null);
      setEditName("");
      setEditDescription("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update group");
    } finally {
      setLoading(false);
    }
  };

  // Delete single group
  const handleDeleteGroup = async (groupId: string | number) => {
    if (!confirm("Are you sure you want to delete this group?")) return;

    setLoading(true);
    setError(null);

    try {
      await deleteGroup(groupId);
      setGroups(groups.filter((g) => g.id !== groupId));
      if (expandedGroup === groupId) {
        setExpandedGroup(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete group");
    } finally {
      setLoading(false);
    }
  };

  // Delete multiple groups
  const handleDeleteSelected = async () => {
    if (selectedGroups.size === 0) return;
    if (
      !confirm(
        `Are you sure you want to delete ${selectedGroups.size} group(s)?`
      )
    )
      return;

    setLoading(true);
    setError(null);

    try {
      await Promise.all(
        Array.from(selectedGroups).map((id) => deleteGroup(id))
      );
      setGroups(groups.filter((g) => !selectedGroups.has(g.id)));
      setSelectedGroups(new Set());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete groups");
    } finally {
      setLoading(false);
    }
  };

  // Handle checkbox selection
  const toggleSelectGroup = (groupId: string | number) => {
    const newSelection = new Set(selectedGroups);
    if (newSelection.has(groupId)) {
      newSelection.delete(groupId);
    } else {
      newSelection.add(groupId);
    }
    setSelectedGroups(newSelection);
  };

  // Select all
  const toggleSelectAll = () => {
    if (selectedGroups.size === groups.length) {
      setSelectedGroups(new Set());
    } else {
      setSelectedGroups(new Set(groups.map((g) => g.id)));
    }
  };

  return (
    <div className="p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h2 className="text-2xl font-bold text-base-content">
              Group Management
            </h2>
            <p className="text-base-content/70 mt-1">
              Create and manage user groups for your organization
            </p>
          </div>
          <button
            className="btn btn-primary gap-2"
            onClick={() => setShowCreateForm(!showCreateForm)}
            disabled={loading}
          >
            <PlusIcon className="w-5 h-5" />
            Create Group
          </button>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="alert alert-error mb-4">
            <XMarkIcon className="w-5 h-5" />
            <span>{error}</span>
            <button
              onClick={() => setError(null)}
              className="btn btn-sm btn-ghost"
            >
              Dismiss
            </button>
          </div>
        )}

        {/* Create Group Inline Form */}
        {showCreateForm && (
          <div className="bg-base-200 rounded-lg shadow-lg p-6 mb-4 border-2 border-primary">
            <h3 className="font-bold text-xl mb-4">Create New Group</h3>
            <div className="grid grid-cols-2 gap-4 mb-4">
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">Group Name *</span>
                </label>
                <input
                  type="text"
                  placeholder="e.g., Engineering Team"
                  className="input input-bordered w-full"
                  value={newGroupName}
                  onChange={(e) => setNewGroupName(e.target.value)}
                  disabled={loading}
                />
              </div>
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">Description</span>
                </label>
                <input
                  type="text"
                  placeholder="Brief description"
                  className="input input-bordered w-full"
                  value={newGroupDescription}
                  onChange={(e) => setNewGroupDescription(e.target.value)}
                  disabled={loading}
                />
              </div>
            </div>
            <div className="flex gap-2 justify-end">
              <button
                className="btn btn-ghost"
                onClick={() => {
                  setShowCreateForm(false);
                  setNewGroupName("");
                  setNewGroupDescription("");
                }}
                disabled={loading}
              >
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={handleCreateGroup}
                disabled={loading || !newGroupName.trim()}
              >
                {loading ? (
                  <span className="loading loading-spinner loading-sm"></span>
                ) : (
                  "Create Group"
                )}
              </button>
            </div>
          </div>
        )}

        {/* Groups Table */}
        <div className="bg-base-200 rounded-lg shadow-lg overflow-hidden">
          <table className="table w-full">
            <thead className="bg-base-300">
              <tr>
                <th>Group Name</th>
                <th>Description</th>
                <th>Members</th>
                <th className="w-32">
                  {selectedGroups.size > 0 && (
                    <button
                      onClick={handleDeleteSelected}
                      className="btn btn-ghost btn-sm text-error"
                      disabled={loading}
                    >
                      <TrashIcon className="w-5 h-5" />
                      Delete ({selectedGroups.size})
                    </button>
                  )}
                </th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody>
              {groups.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="text-center py-8 text-base-content/70"
                  >
                    No groups found. Create your first group to get started.
                  </td>
                </tr>
              ) : (
                groups.map((group) => (
                  <React.Fragment key={group.id}>
                    {/* Main Row */}
                    <tr
                      className={`hover cursor-pointer ${
                        expandedGroup === group.id ? "bg-base-300" : ""
                      }`}
                      onClick={() => toggleExpand(group.id)}
                    >
                      <td className="font-semibold">{group.name}</td>
                      <td className="text-base-content/70">
                        {group.description || "No description"}
                      </td>
                      <td>
                        <div className="flex items-center gap-2">
                          <UserGroupIcon className="w-4 h-4" />
                          <span className="badge badge-ghost">
                            {/* TODO: Display actual member count when API is ready */}
                            0
                          </span>
                        </div>
                      </td>
                      <td onClick={(e) => e.stopPropagation()}>
                        <div className="flex gap-2">
                          <button
                            className="btn btn-ghost btn-sm"
                            onClick={() => {
                              setExpandedGroup(group.id);
                              startEdit(group);
                            }}
                            disabled={loading}
                          >
                            <PencilIcon className="w-5 h-5 text-info" />
                          </button>
                          <button
                            className="btn btn-ghost btn-sm"
                            onClick={() => handleDeleteGroup(group.id)}
                            disabled={loading}
                          >
                            <TrashIcon className="w-5 h-5 text-error" />
                          </button>
                        </div>
                      </td>
                      <td>
                        {expandedGroup === group.id ? (
                          <ChevronUpIcon className="w-5 h-5" />
                        ) : (
                          <ChevronDownIcon className="w-5 h-5" />
                        )}
                      </td>
                    </tr>

                    {/* Expanded Row */}
                    {expandedGroup === group.id && (
                      <tr>
                        <td colSpan={6} className="bg-base-100 p-0">
                          <div className="p-6 border-t-2 border-primary/20">
                            {/* Edit Mode */}
                            {editingGroup === group.id ? (
                              <div className="space-y-4">
                                <div className="flex justify-between items-center mb-4">
                                  <h4 className="font-bold text-lg">
                                    Edit Group Details
                                  </h4>
                                  <div className="flex gap-2">
                                    <button
                                      className="btn btn-sm btn-ghost"
                                      onClick={cancelEdit}
                                      disabled={loading}
                                    >
                                      Cancel
                                    </button>
                                    <button
                                      className="btn btn-sm btn-primary"
                                      onClick={() =>
                                        handleUpdateGroup(group.id)
                                      }
                                      disabled={loading || !editName.trim()}
                                    >
                                      {loading ? (
                                        <span className="loading loading-spinner loading-sm"></span>
                                      ) : (
                                        <>
                                          <CheckIcon className="w-4 h-4" />
                                          Save Changes
                                        </>
                                      )}
                                    </button>
                                  </div>
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                  <div className="form-control">
                                    <label className="label">
                                      <span className="label-text font-semibold">
                                        Group Name
                                      </span>
                                    </label>
                                    <input
                                      type="text"
                                      className="input input-bordered w-full"
                                      value={editName}
                                      onChange={(e) =>
                                        setEditName(e.target.value)
                                      }
                                      disabled={loading}
                                    />
                                  </div>
                                  <div className="form-control">
                                    <label className="label">
                                      <span className="label-text font-semibold">
                                        Description
                                      </span>
                                    </label>
                                    <input
                                      type="text"
                                      className="input input-bordered w-full"
                                      value={editDescription}
                                      onChange={(e) =>
                                        setEditDescription(e.target.value)
                                      }
                                      disabled={loading}
                                    />
                                  </div>
                                </div>
                              </div>
                            ) : (
                              <div className="flex justify-between items-start mb-4">
                                <div>
                                  <h4 className="font-bold text-lg">
                                    {group.name}
                                  </h4>
                                  <p className="text-base-content/70">
                                    {group.description || "No description"}
                                  </p>
                                </div>
                                <button
                                  className="btn btn-sm btn-ghost"
                                  onClick={() => startEdit(group)}
                                  disabled={loading}
                                >
                                  <PencilIcon className="w-4 h-4" />
                                  Edit Details
                                </button>
                              </div>
                            )}

                            {/* Member Management Section - Placeholder for future */}
                            <div className="divider my-4">
                              Member Management
                            </div>
                            <div className="alert alert-info">
                              <svg
                                xmlns="http://www.w3.org/2000/svg"
                                fill="none"
                                viewBox="0 0 24 24"
                                className="stroke-current shrink-0 w-6 h-6"
                              >
                                <path
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                                  strokeWidth="2"
                                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                                ></path>
                              </svg>
                              <span>
                                Member management will be available soon. You'll
                                be able to add and remove users from groups
                                here.
                              </span>
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default InlineGroupsTable;
