// src/app/(home)/organization_management/users/UsersHeaderStats.tsx

import React from "react";
import {
  EnvelopeIcon,
  UserGroupIcon,
  UserIcon,
} from "@heroicons/react/24/outline";

/* -------------------------------------------------------------------------- */
/*                             Header & Stats Block                           */
/* -------------------------------------------------------------------------- */

interface UsersHeaderStatsProps {
  activeUserCount: number;
  pendingCount: number;
  totalCount: number;
  loading: boolean;
  onInviteClick: () => void;
}

const UsersHeaderStats: React.FC<UsersHeaderStatsProps> = ({
  activeUserCount,
  pendingCount,
  totalCount,
  loading,
  onInviteClick,
}) => {
  return (
    <>
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold">Organization Users</h2>
          <p className="text-base-content/70 text-sm mt-1">
            Manage users in your organization. Invite new users via email or add
            them directly.
          </p>
        </div>
        <button
          className="btn btn-primary gap-2"
          onClick={onInviteClick}
          disabled={loading}
        >
          <UserIcon className="w-5 h-5" />
          Invite User
        </button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="stat bg-base-200 rounded-lg">
          <div className="stat-figure text-primary">
            <UserIcon className="w-8 h-8" />
          </div>
          <div className="stat-title">Active Users</div>
          <div className="stat-value text-primary">{activeUserCount}</div>
        </div>
        <div className="stat bg-base-200 rounded-lg">
          <div className="stat-figure text-warning">
            <EnvelopeIcon className="w-8 h-8" />
          </div>
          <div className="stat-title">Pending Invites</div>
          {/* <div className="stat-value text-warning">{pendingCount}</div> */}
          <div className="stat-value text-warning">0</div>
        </div>
        <div className="stat bg-base-200 rounded-lg">
          <div className="stat-figure text-secondary">
            <UserGroupIcon className="w-8 h-8" />
          </div>
          <div className="stat-title">Total</div>
          <div className="stat-value text-secondary">{totalCount}</div>
        </div>
      </div>
    </>
  );
};

export default UsersHeaderStats;
