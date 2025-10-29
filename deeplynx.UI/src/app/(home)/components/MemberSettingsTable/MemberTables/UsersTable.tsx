import React, { useEffect, useState } from "react";
import GenericTable from "../../GenericTable";
import { useLanguage } from "@/app/contexts/Language";
import { Column } from "../../../types/types";
import { TrashIcon, PencilIcon } from "@heroicons/react/24/outline";
import { getAllUsers, updateUser, deleteUser } from "@/app/lib/user_services.client";
import EditSysUser from "../MemberModals/EditSysUser";
import MemberManagementUserSkeleton from "../../skeletons/membermanagementuserskeleton";
import { UserResponseDto } from "@/app/(home)/types/responseDTOs";
const UsersTable = () => {
  const { t } = useLanguage();
  const [data, setData] = useState<UserResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>([]);
  const [selectAll, setSelectAll] = useState(false);
  const [editSysUserModal, setEditSysUserModal] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [selectedUserName, setSelectedUserName] = useState<string>("");

  const fetchUsers = async () => {
    try {
      const users = await getAllUsers();
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
  }, []);

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
    } catch (err) {
      console.error("Failed to delete user:", err);
      setError("Failed to delete user.");
    }
  };

  const handleDeleteSelected = async () => {
    const selectedUserIds = data.filter((_, i) => selectedMembers[i]).map(user => user.id);
    try {
      await Promise.all(selectedUserIds.map(userId => deleteUser(userId)));
      setData((prev) => prev.filter((_, i) => !selectedMembers[i]));
    } catch (err) {
      console.error("Failed to delete selected users:", err);
      setError("Failed to delete selected users.");
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
    <div>
      <GenericTable columns={columns} data={data} enablePagination />
      {selectedUserId !== null && (
        <EditSysUser
          isOpen={editSysUserModal}
          onClose={() => setEditSysUserModal(false)}
          userId={selectedUserId}
          userName={selectedUserName}
        />
      )}
    </div>
  );
};

export default UsersTable;