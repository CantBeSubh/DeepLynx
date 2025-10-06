'use client';
import React, { FC, useEffect, useState } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column, ProjectMembersTable } from '../../../types/types';
import RoleSwap from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/RoleSwap";
import { TrashIcon } from '@heroicons/react/24/outline';
import { removeProjectMemberRole, updateProjectMemberRole } from '@/app/lib/projects_services.client';

interface MembersTableProps {
  data: ProjectMembersTable[];
  projectId: string | null;
}

const MembersTable: FC<MembersTableProps> = ({ data: initialData, projectId }) => {
  const { t } = useLanguage();
  const [data, setData] = useState<ProjectMembersTable[]>(initialData);
  const [addRoleSwap, setAddRoleSwap] = useState<boolean>(false);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>(
    new Array(initialData?.length || 0).fill(false)
  );
  const [selectAll, setSelectAll] = useState<boolean>(false);
  const [selectedMemberForRoleSwap, setSelectedMemberForRoleSwap] = useState<ProjectMembersTable | null>(null);

  useEffect(() => {
    setData(initialData);
    setSelectedMembers(new Array(initialData?.length || 0).fill(false));
    setSelectAll(false);
  }, [initialData]);

  const handleSelectAll = () => {
    const newSelection = !selectAll;
    setSelectAll(newSelection);
    setSelectedMembers(new Array(data.length).fill(newSelection));
  };

  const handleCheckboxChange = (index: number) => {
    const newSelection = [...selectedMembers];
    newSelection[index] = !newSelection[index];
    setSelectedMembers(newSelection);

    if (newSelection.every(Boolean)) {
      setSelectAll(true);
    } else {
      setSelectAll(false);
    }
  };

  const handleDelete = async (row: ProjectMembersTable, index: number) => {
    const memberToDelete = data[index];
    console.log(data);

    try {
      await removeProjectMemberRole(
        Number(projectId),
        memberToDelete.memberId || undefined,
        memberToDelete.groupId || undefined
      );

      const newData = data.filter((_, i) => i !== index);
      setData(newData);
      setSelectedMembers(new Array(newData.length).fill(false));
      setSelectAll(false);
    } catch (error) {
      console.error('Failed to delete member:', error);
    }
  };

  const handleDeleteSelected = async () => {
    const membersToDelete = data.filter((_, index) => selectedMembers[index]);

    try {
      // Delete all selected members
      await Promise.all(
        membersToDelete.map(member =>
          removeProjectMemberRole(
            Number(projectId),
            member.memberId || undefined,
            member.groupId || undefined
          )
        )
      );

      const newData = data.filter((_, index) => !selectedMembers[index]);
      setData(newData);
      setSelectedMembers(new Array(newData.length).fill(false));
      setSelectAll(false);
    } catch (error) {
      console.error('Failed to delete selected members:', error);
    }
  };

  const handleRoleSwapOpen = (member: ProjectMembersTable) => {
    setSelectedMemberForRoleSwap(member);
    setAddRoleSwap(true);
  };

  const handleRoleSwapClose = () => {
    setSelectedMemberForRoleSwap(null);
    setAddRoleSwap(false);
  };

  const handleRoleUpdate = async (newRoleId: number) => {
    if (!selectedMemberForRoleSwap || !projectId) return;

    try {
      await updateProjectMemberRole(
        Number(projectId),
        newRoleId,
        selectedMemberForRoleSwap.memberId || undefined,
        selectedMemberForRoleSwap.groupId || undefined
      );

      // Update local state
      const updatedData = data.map(member =>
        member.memberId === selectedMemberForRoleSwap.memberId
          ? { ...member, roleId: newRoleId }
          : member
      );
      setData(updatedData);
      handleRoleSwapClose();
    } catch (error) {
      console.error('Failed to update role:', error);
    }
  };

  const multipleSelected = () => {
    return selectedMembers.filter(selected => selected).length > 1;
  };

  const columns: Column<ProjectMembersTable>[] = [
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (row: ProjectMembersTable, index: number) => (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectedMembers[index]}
          onChange={() => handleCheckboxChange(index)}
        />
      ),
      sortable: false,
    },
    {
      header: "Name",
      data: "name",
    },
    {
      header: "Email",
      data: "email",
    },
    {
      header: "Role",
      data: "role",
    },
    {
      header: (
        <div className="flex">
          {multipleSelected() && (
            <button className="btn" onClick={() => setAddRoleSwap(true)}>
              {t.translations.ROLE}
            </button>
          )}
        </div>
      ),
      cell: (row: ProjectMembersTable) => (
        <div className="flex">
          <button
            className="btn"
            onClick={() => handleRoleSwapOpen(row)}
          >
            {t.translations.ROLE}
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
      cell: (row: ProjectMembersTable, index: number) => (
        <div className="flex">
          <button onClick={() => handleDelete(row, index)}>
            <TrashIcon className="size-6 text-red-500" />
          </button>
        </div>
      ),
      sortable: false,
    },
  ];

  return (
    <div>
      <GenericTable
        columns={columns}
        data={data}
        enablePagination
      />

      <RoleSwap
        isOpen={addRoleSwap}
        onClose={handleRoleSwapClose}
      // onRoleUpdate={handleRoleUpdate}
      // currentMember={selectedMemberForRoleSwap}
      />
    </div>
  );
};

export default MembersTable;