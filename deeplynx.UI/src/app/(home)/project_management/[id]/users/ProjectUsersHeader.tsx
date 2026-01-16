"use client";

import {
  UserGroupIcon,
  UserIcon,
  UsersIcon,
} from "@heroicons/react/24/outline";
import React from "react";

/* -------------------------------------------------------------------------- */
/*                             Header & Stats Block                           */
/* -------------------------------------------------------------------------- */

interface ProjectUsersHeaderProps {
  totalMembers: number;
  userCount: number;
  groupCount: number;
  loading: boolean;
  onAddUser: () => void;
  onAddGroup: () => void;
}

const ProjectUsersHeader: React.FC<ProjectUsersHeaderProps> = ({
  totalMembers,
  userCount,
  groupCount,
  loading,
  onAddUser,
  onAddGroup,
}) => {
  return (
    <>
      {/* Header */}
      <div className="flex justify-between items-center mb-6 border-b border-base-300 pb-4">
        <div>
          <h2 className="text-2xl font-bold">Project Members</h2>
          <p className="text-base-content/70 mt-1">
            Manage users and groups assigned to this project. A role is required
            for each member.
          </p>
        </div>
        <div className="flex gap-2">
          <button
            className="btn btn-outline btn-sm"
            disabled={loading}
            onClick={onAddGroup}
          >
            Add Group
          </button>
          <button
            className="btn btn-primary btn-sm"
            disabled={loading}
            onClick={onAddUser}
          >
            Add User
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="stat bg-base-200 rounded-lg text-secondary border-none">
          <div className="stat-title">Total Members</div>
          <div className="flex justify-between">
            <div className="stat-value">{totalMembers}</div>
            <UserGroupIcon className="size-8" />
          </div>
        </div>

        <div className="stat bg-base-200 rounded-lg text-secondary border-none">
          <div className="stat-title">Users</div>
          <div className="flex justify-between">
            <div className="stat-value">{userCount}</div>
            <UserIcon className="size-8" />
          </div>
        </div>

        <div className="stat bg-base-200 rounded-lg text-secondary border-none">
          <div className="stat-title">Groups</div>
          <div className="flex justify-between">
            <div className="stat-value">{groupCount}</div>
            <UsersIcon className="size-8" />
          </div>
        </div>
      </div>
    </>
  );
};

export default ProjectUsersHeader;
