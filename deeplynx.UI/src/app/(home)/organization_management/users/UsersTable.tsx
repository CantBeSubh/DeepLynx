// src/app/(home)/organization_management/users/UsersTable.tsx

import React, { useEffect, useState } from "react";
import GenericTable from "../../components/GenericTable";
import { useLanguage } from "@/app/contexts/Language";
import { Column } from "../../types/types";
import { TrashIcon, PencilIcon, PlusIcon } from "@heroicons/react/24/outline";
import {
  getAllUsers,
  deleteUser,
} from "@/app/lib/client_service/user_services.client";
import EditSysUser from "../../components/SiteManagementPortal/EditSysUser";
import MemberManagementUserSkeleton from "../../components/skeletons/membermanagementuserskeleton";
import { UserResponseDto } from "@/app/(home)/types/responseDTOs";
import InviteUserModal from "./InviteUserModal";
import toast from "react-hot-toast";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { sendEmail } from "@/app/lib/client_service/notification_services.client";

interface Props {
  members: UserResponseDto[];
}

const UsersTable = ({ members }: Props) => {
  const { t } = useLanguage();
  const [data, setData] = useState<UserResponseDto[]>(members);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>([]);
  const [selectAll, setSelectAll] = useState(false);
  const [editSysUserModal, setEditSysUserModal] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [selectedUserName, setSelectedUserName] = useState<string>("");
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const { organization } = useOrganizationSession();

  const handleInviteUser = async (email: string) => {
    try {
      await sendEmail(email, "New User");
      toast.success(`Invitation sent to ${email}`);
      fetchUsers();
    } catch (error) {
      console.error("Error inviting user:", error);
      toast.error("Failed to send invitation");
      throw error;
    }
  };

  const fetchUsers = async () => {
    try {
      // Filter by organization ID
      const users = await getAllUsers(organization?.organizationId);
      setData(users);
    } catch (err) {
      console.error(err);
      setError("Failed to load users.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [organization?.organizationId]); // Re-fetch when organization changes

  useEffect(() => {
    setSelectedMembers(new Array(data.length).fill(false));
    setSelectAll(false);
  }, [data.length]);

  const handleSelectAll = () => {
    const next = !selectAll;
    setSelectAll(next);
    setSelectedMembers(new Array(data.length).fill(next));
  };

  const handleCheckboxChange = (index: number) => {
    const next = [...selectedMembers];
    next[index] = !next[index];
    setSelectedMembers(next);
    setSelectAll(next.every(Boolean));
  };

  const handleDelete = async (index: number) => {
    const userId = data[index].id;
    try {
      await deleteUser(userId);
      setData((prev) => prev.filter((_, i) => i !== index));
      toast.success("User deleted successfully");
    } catch (err) {
      console.error("Failed to delete user:", err);
      toast.error("Failed to delete user");
    }
  };

  const handleDeleteSelected = async () => {
    const selectedUserIds = data
      .filter((_, i) => selectedMembers[i])
      .map((user) => user.id);
    try {
      await Promise.all(selectedUserIds.map((userId) => deleteUser(userId)));
      setData((prev) => prev.filter((_, i) => !selectedMembers[i]));
      toast.success(`${selectedUserIds.length} users deleted successfully`);
    } catch (err) {
      console.error("Failed to delete selected users:", err);
      toast.error("Failed to delete selected users");
    }
  };

  const multipleSelected = () => selectedMembers.filter(Boolean).length > 1;

  const openEditModal = (userId: number, userName: string) => {
    setSelectedUserId(userId);
    setSelectedUserName(userName);
    setEditSysUserModal(true);
  };

  const columns: Column<UserResponseDto>[] = [
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (_row, index) => (
        <input
          type="checkbox"
          className="checkbox"
          checked={!!selectedMembers[index]}
          onChange={() => handleCheckboxChange(index)}
        />
      ),
      sortable: false,
    },
    { header: "Name", data: "name" },
    { header: "Email", data: "email" },
    {
      header: "",
      cell: (row) => (
        <div className="flex">
          <button onClick={() => openEditModal(row.id, row.name)}>
            <PencilIcon className="size-6 text-secondary" />
          </button>
        </div>
      ),
      sortable: false,
    },
    {
      header: (
        <div className="flex">
          {multipleSelected() && (
            <button onClick={handleDeleteSelected}>
              <TrashIcon className="size-6 text-red-500" />
            </button>
          )}
        </div>
      ),
      cell: (_row, index) => (
        <div className="flex">
          <button onClick={() => handleDelete(index)}>
            <TrashIcon className="size-6 text-red-500" />
          </button>
        </div>
      ),
      sortable: false,
    },
  ];

  if (loading) return <MemberManagementUserSkeleton />;
  if (error) return <div className="p-4 text-red-500">{error}</div>;

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <h1 className="text-2xl font-bold">Organization Users</h1>
          <div className="flex gap-2">
            <button
              className="btn btn-primary btn-sm gap-2"
              onClick={() => setIsInviteModalOpen(true)}
            >
              <PlusIcon className="size-5" />
              Invite User
            </button>
          </div>
        </div>
        <p className="text-base-content/70">
          Manage users in your organization. Invite new users via email or add
          them directly. Note: User roles are assigned at the project level, not
          at the organization level.
        </p>
      </div>

      <GenericTable columns={columns} data={data} enablePagination />

      {/* Modals */}
      {selectedUserId !== null && (
        <EditSysUser
          isOpen={editSysUserModal}
          onClose={() => setEditSysUserModal(false)}
          userId={selectedUserId}
          userName={selectedUserName}
          onUserUpdated={fetchUsers}
        />
      )}
      <InviteUserModal
        isOpen={isInviteModalOpen}
        onClose={() => setIsInviteModalOpen(false)}
        onSubmit={handleInviteUser}
        organizationName={organization?.organizationName || "your organization"}
      />
    </div>
  );
};

export default UsersTable;