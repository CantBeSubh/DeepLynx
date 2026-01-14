// src/app/(home)/project_management/[id]/users/ProjectUsersHeader.tsx

import React from "react";
import {
  UserIcon,
  UserGroupIcon,
  UsersIcon,
  EnvelopeIcon,
} from "@heroicons/react/24/outline";

/* -------------------------------------------------------------------------- */
/*                        Project Users Header Component                      */
/* -------------------------------------------------------------------------- */

interface ProjectUsersHeaderProps {
  totalMembers: number;
  userCount: number;
  groupCount: number;
  loading: boolean;
  onAddUser: () => void;
  onAddGroup: () => void;
  onInviteUser: () => void;
}

const ProjectUsersHeader: React.FC<ProjectUsersHeaderProps> = ({
  totalMembers,
  userCount,
  groupCount,
  loading,
  onAddUser,
  onAddGroup,
  onInviteUser,
}) => {
  return (
    <>
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold">Project Members</h2>
          <p className="text-base-content/70 text-sm mt-1">
            Manage users and groups with access to this project
          </p>
        </div>
        <div className="flex gap-2">
          <button
            className="btn btn-outline btn-primary gap-2"
            onClick={onInviteUser}
            disabled={loading}
          >
            <EnvelopeIcon className="w-5 h-5" />
            Invite User
          </button>
          <div className="dropdown dropdown-end">
            <button
              tabIndex={0}
              className="btn btn-primary gap-2"
              disabled={loading}
            >
              <UserIcon className="w-5 h-5" />
              Add Member
            </button>
            <ul
              tabIndex={0}
              className="dropdown-content menu bg-base-100 rounded-box z-[1] w-52 p-2 shadow-lg border border-base-300"
            >
              <li>
                <a onClick={onAddUser}>
                  <UserIcon className="w-4 h-4" />
                  Add Existing User
                </a>
              </li>
              <li>
                <a onClick={onAddGroup}>
                  <UserGroupIcon className="w-4 h-4" />
                  Add Group
                </a>
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="stat bg-base-200 rounded-lg">
          <div className="stat-figure text-secondary">
            <UsersIcon className="w-8 h-8" />
          </div>
          <div className="stat-title">Total Members</div>
          <div className="stat-value text-secondary">{totalMembers}</div>
          <div className="stat-desc">Users + Groups</div>
        </div>
        <div className="stat bg-base-200 rounded-lg">
          <div className="stat-figure text-primary">
            <UserIcon className="w-8 h-8" />
          </div>
          <div className="stat-title">Users</div>
          <div className="stat-value text-primary">{userCount}</div>
          <div className="stat-desc">Individual members</div>
        </div>
        <div className="stat bg-base-200 rounded-lg">
          <div className="stat-figure text-accent">
            <UserGroupIcon className="w-8 h-8" />
          </div>
          <div className="stat-title">Groups</div>
          <div className="stat-value text-accent">{groupCount}</div>
          <div className="stat-desc">Group memberships</div>
        </div>
      </div>
    </>
  );
};

export default ProjectUsersHeader;
