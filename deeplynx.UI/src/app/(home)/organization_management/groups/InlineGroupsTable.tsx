// src/app/(home)/organization_management/groups/InlineGroupsTable.tsx

"use client";

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  addUserToGroup,
  createGroup,
  deleteGroup,
  getGroupMembers,
  removeUserFromGroup,
  updateGroup,
} from "@/app/lib/client_service/group_services.client";
import {
  CheckIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  PencilIcon,
  PlusIcon,
  TrashIcon,
  UserGroupIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import React, { useState } from "react";
import toast from "react-hot-toast";
import AvatarCell from "../../components/Avatar";
import {
  CreateGroupRequestDto,
  UpdateGroupRequestDto,
} from "../../types/requestDTOs";
import { GroupResponseDto, UserResponseDto } from "../../types/responseDTOs";

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

  console.log("Inline table members: ", availableUsers);

  // Member management state
  const [groupMembers, setGroupMembers] = useState<
    Map<string | number, UserResponseDto[]>
  >(new Map());
  const [loadingMembers, setLoadingMembers] = useState<Set<string | number>>(
    new Set()
  );
  const [memberSearchTerm, setMemberSearchTerm] = useState("");

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
  const { organization, hasLoaded } = useOrganizationSession();

  // Fetch members for a group
  const fetchGroupMembers = async (groupId: string | number) => {
    // Check if we already have the members cached
    if (groupMembers.has(groupId)) {
      return;
    }

    const newLoadingMembers = new Set(loadingMembers).add(groupId);
    setLoadingMembers(newLoadingMembers);

    try {
      const members = await getGroupMembers(
        organization?.organizationId as number,
        groupId as number
      );
      setGroupMembers(new Map(groupMembers).set(groupId, members));
    } catch (err) {
      console.error("Failed to fetch group members:", err);
      setError(err instanceof Error ? err.message : "Failed to fetch members");
    } finally {
      const updatedLoadingMembers = new Set(loadingMembers);
      updatedLoadingMembers.delete(groupId);
      setLoadingMembers(updatedLoadingMembers);
    }
  };

  // Toggle expand/collapse and fetch members
  const toggleExpand = async (groupId: string | number) => {
    if (expandedGroup === groupId) {
      setExpandedGroup(null);
      setEditingGroup(null);
    } else {
      setExpandedGroup(groupId);
      setEditingGroup(null);
      // Fetch members when expanding
      await fetchGroupMembers(groupId);
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
    const dto: CreateGroupRequestDto = {
      name: newGroupName,
      description: newGroupDescription,
    };
    try {
      const newGroup = await createGroup(organizationId as number, dto);
      setGroups([...groups, newGroup]);
      setNewGroupName("");
      setNewGroupDescription("");
      setShowCreateForm(false);
      toast.success("Group created");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create group");
      toast.error("Group not created");
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
    const dto: UpdateGroupRequestDto = {
      name: editName,
      description: editDescription,
    };
    try {
      const updatedGroup = await updateGroup(
        organization?.organizationId as number,
        groupId as number,
        dto
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
      await deleteGroup(
        organization?.organizationId as number,
        groupId as number
      );
      setGroups(groups.filter((g) => g.id !== groupId));
      if (expandedGroup === groupId) {
        setExpandedGroup(null);
      }
      // Clear cached members
      const newGroupMembers = new Map(groupMembers);
      newGroupMembers.delete(groupId);
      setGroupMembers(newGroupMembers);
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
        Array.from(selectedGroups).map((id) =>
          deleteGroup(organization?.organizationId as number, id as number)
        )
      );
      setGroups(groups.filter((g) => !selectedGroups.has(g.id)));
      setSelectedGroups(new Set());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete groups");
    } finally {
      setLoading(false);
    }
  };

  // Add member to group
  const handleAddMember = async (
    groupId: string | number,
    userId: string | number
  ) => {
    setLoading(true);
    setError(null);

    try {
      await addUserToGroup(
        organization?.organizationId as number,
        groupId as number,
        userId as number
      );
      // Refetch members after adding
      const members = await getGroupMembers(
        organization?.organizationId as number,
        groupId as number
      );
      setGroupMembers(new Map(groupMembers).set(groupId, members));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to add member");
    } finally {
      setLoading(false);
    }
  };

  // Remove member from group
  const handleRemoveMember = async (
    groupId: string | number,
    userId: string | number
  ) => {
    setLoading(true);
    setError(null);

    try {
      await removeUserFromGroup(
        organization?.organizationId as number,
        groupId as number,
        userId as number
      );
      // Refetch members after removing
      const members = await getGroupMembers(
        organization?.organizationId as number,
        groupId as number
      );
      setGroupMembers(new Map(groupMembers).set(groupId, members));
      toast.success("Removed member");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove member");
      toast.error("Falied to remove member");
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

  // Get current members for a group
  const getCurrentMembers = (groupId: string | number): UserResponseDto[] => {
    return groupMembers.get(groupId) || [];
  };

  // Get available users (not in group)
  const getAvailableUsers = (groupId: string | number): UserResponseDto[] => {
    const currentMembers = getCurrentMembers(groupId);
    const memberIds = new Set(currentMembers.map((m) => m.id));
    return availableUsers.filter(
      (u) => !memberIds.has(u.id) && u.isActive && !u.isArchived
    );
  };

  // Filter users by search term
  const filterUsersBySearch = (users: UserResponseDto[]): UserResponseDto[] => {
    if (!memberSearchTerm.trim()) return users;

    const searchLower = memberSearchTerm.toLowerCase();
    return users.filter(
      (u) =>
        u.name?.toLowerCase().includes(searchLower) ||
        u.email?.toLowerCase().includes(searchLower)
    );
  };

  return (
    <div className="p-6">
      <div className="mx-auto">
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
            <PlusIcon className="size-6" />
            Create Group
          </button>
        </div>

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
        <div className=" rounded-lg shadow-xl overflow-hidden border-2 border-primary ">
          <table className="table w-full">
            <thead className="">
              <tr>
                <th className="w-12">
                  <input
                    type="checkbox"
                    className="checkbox checkbox-sm"
                    checked={
                      selectedGroups.size === groups.length && groups.length > 0
                    }
                    onChange={toggleSelectAll}
                  />
                </th>
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
                      <TrashIcon className="size-6" />
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
                groups.map((group) => {
                  const currentMembers = getCurrentMembers(group.id);
                  const availableUsersForGroup = getAvailableUsers(group.id);
                  const filteredAvailable = filterUsersBySearch(
                    availableUsersForGroup
                  );
                  const isLoadingMembers = loadingMembers.has(group.id);

                  return (
                    <React.Fragment key={group.id}>
                      {/* Main Row */}
                      <tr
                        className={`hover cursor-pointer ${
                          expandedGroup === group.id ? "bg-base-200" : ""
                        }`}
                        onClick={() => toggleExpand(group.id)}
                      >
                        <td onClick={(e) => e.stopPropagation()}>
                          <input
                            type="checkbox"
                            className="checkbox checkbox-sm"
                            checked={selectedGroups.has(group.id)}
                            onChange={() => toggleSelectGroup(group.id)}
                          />
                        </td>
                        <td className="font-semibold">{group.name}</td>
                        <td className="text-base-content/70">
                          {group.description || "No description"}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <UserGroupIcon className="size-6" />
                            <span className="badge badge-ghost">
                              {group.memberCount}
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
                              <PencilIcon className="size-6" />
                            </button>
                            <button
                              className="btn btn-ghost btn-sm"
                              onClick={() => handleDeleteGroup(group.id)}
                              disabled={loading}
                            >
                              <TrashIcon className="size-6 text-error" />
                            </button>
                          </div>
                        </td>
                        <td>
                          {expandedGroup === group.id ? (
                            <ChevronUpIcon className="size-6" />
                          ) : (
                            <ChevronDownIcon className="size-6" />
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

                              {/* Member Management Section */}
                              <div className="divider my-4">
                                Member Management
                              </div>

                              {isLoadingMembers ? (
                                <div className="flex justify-center items-center py-8">
                                  <span className="loading loading-spinner loading-lg"></span>
                                </div>
                              ) : (
                                <div className="grid grid-cols-2 gap-6 max-h-96">
                                  {/* Current Members */}
                                  <div className="flex flex-col">
                                    <div className="flex justify-between items-center mb-3">
                                      <h5 className="font-semibold flex items-center gap-2">
                                        <UserGroupIcon className="w-5 h-5" />
                                        Current Members ({currentMembers.length}
                                        )
                                      </h5>
                                    </div>
                                    <div className="space-y-2 max-h-80 overflow-y-auto">
                                      {currentMembers.length === 0 ? (
                                        <div className="text-center py-4 text-base-content/60">
                                          No members in this group yet
                                        </div>
                                      ) : (
                                        currentMembers.map((user) => (
                                          <div
                                            key={user.id}
                                            className="flex items-center justify-between p-3 bg-base-200 rounded-lg hover:bg-base-300 transition"
                                          >
                                            <div className="flex items-center gap-3">
                                              <AvatarCell name={user.name} />
                                              <div>
                                                <p className="font-medium">
                                                  {user.name}
                                                </p>
                                                <p className="text-sm text-base-content/60">
                                                  {user.email}
                                                </p>
                                              </div>
                                            </div>
                                            <button
                                              className="btn btn-ghost btn-sm text-error hover:bg-error/20"
                                              onClick={() =>
                                                handleRemoveMember(
                                                  group.id,
                                                  user.id!
                                                )
                                              }
                                              disabled={loading}
                                            >
                                              <XMarkIcon className="size-6" />
                                            </button>
                                          </div>
                                        ))
                                      )}
                                    </div>
                                  </div>

                                  {/* Add Members */}
                                  <div className="flex flex-col">
                                    <div className="flex justify-between items-center mb-3">
                                      <h5 className="font-semibold">
                                        Add Members
                                      </h5>
                                    </div>
                                    <div className="form-control mb-3">
                                      <div className="input-group">
                                        <span className="bg-base-300"></span>
                                        <input
                                          type="text"
                                          placeholder="Search users..."
                                          className="input input-bordered w-full"
                                          value={memberSearchTerm}
                                          onChange={(e) =>
                                            setMemberSearchTerm(e.target.value)
                                          }
                                        />
                                      </div>
                                    </div>
                                    <div className="space-y-2 max-h-64 overflow-y-auto">
                                      {filteredAvailable.length === 0 ? (
                                        <div className="text-center py-4 text-base-content/60">
                                          {availableUsersForGroup.length === 0
                                            ? "All users are already in this group"
                                            : "No users found"}
                                        </div>
                                      ) : (
                                        filteredAvailable.map((user) => (
                                          <div
                                            key={user.id}
                                            className="flex items-center justify-between p-3 bg-base-200 rounded-lg hover:bg-base-300 transition"
                                          >
                                            <div className="flex items-center gap-3">
                                              <AvatarCell name={user.name} />
                                              <div>
                                                <p className="font-medium">
                                                  {user.name}
                                                </p>
                                                <p className="text-sm text-base-content/60">
                                                  {user.email}
                                                </p>
                                              </div>
                                            </div>
                                            <button
                                              className="btn btn-ghost btn-sm text-success hover:bg-success/20"
                                              onClick={() =>
                                                handleAddMember(
                                                  group.id,
                                                  user.id!
                                                )
                                              }
                                              disabled={loading}
                                            >
                                              <PlusIcon className="size-6" />
                                            </button>
                                          </div>
                                        ))
                                      )}
                                    </div>
                                  </div>
                                </div>
                              )}
                            </div>
                          </td>
                        </tr>
                      )}
                    </React.Fragment>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default InlineGroupsTable;
