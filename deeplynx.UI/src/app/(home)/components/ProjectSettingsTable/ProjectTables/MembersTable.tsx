'use client';
import React, { FC, useEffect, useState } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column } from '../../../types/types';
import RoleSwap from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/RoleSwap";
import { TrashIcon } from '@heroicons/react/24/outline';
import { removeProjectMemberRole, updateProjectMemberRole } from '@/app/lib/projects_services.client';
import { ProjectMembersDto } from '@/app/(home)/types/responseDTOs';
import { Role } from '@/app/(home)/types/types';

// interface Role {
//   id: number;
//   name: string;
//   description: string | null;
//   lastUpdatedAt: string;
//   lastUpdatedBy: string | null;
//   isArchived: boolean;
//   projectId: number;
//   organizationId: number | null;
// }

interface MembersTableProps {
  data: ProjectMembersDto[];
  projectId: string | null;
  roles?: Role[]; // Add roles prop
}

const MembersTable: FC<MembersTableProps> = ({ data: initialData, projectId, roles }) => {
  const { t } = useLanguage();
  const [data, setData] = useState<ProjectMembersDto[]>(initialData);
  const [addRoleSwap, setAddRoleSwap] = useState<boolean>(false);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>(
    new Array(initialData?.length || 0).fill(false)
  );
  const [selectAll, setSelectAll] = useState<boolean>(false);
  const [selectedMemberForRoleSwap, setSelectedMemberForRoleSwap] = useState<ProjectMembersDto | null>(null);

  // Helper function to get role name from role ID
  const getRoleName = (roleId: number): string => {
    const role = roles?.find(r => r.id === roleId);
    return role?.name || 'Unknown';
  };

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

  const handleDelete = async (row: ProjectMembersDto, index: number) => {
    const memberToDelete = data[index];
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

  const handleRoleSwapOpen = (member: ProjectMembersDto) => {
    setSelectedMemberForRoleSwap(member);
    setAddRoleSwap(true);
  };

  const handleRoleSwapClose = () => {
    setSelectedMemberForRoleSwap(null);
    setAddRoleSwap(false);
  };

  const handleRoleUpdate = async (newRoleId: number, newRoleName?: string) => {
    if (!projectId) return;

    // Get the role name from the roles array if not provided
    const roleName = newRoleName || getRoleName(newRoleId);


    try {
      if (multipleSelected()) {
        // Bulk update for multiple selected members
        const membersToUpdate = data.filter((_, index) => selectedMembers[index]);

        await Promise.all(
          membersToUpdate.map(member =>
            updateProjectMemberRole(
              Number(projectId),
              newRoleId,
              member.memberId || undefined,
              member.groupId || undefined
            )
          )
        );

        // Update local state for all updated members
        const updatedData = data.map(member => {
          const isSelected = membersToUpdate.some(m =>
            m.memberId === member.memberId && m.groupId === member.groupId
          );
          return isSelected ? {
            ...member,
            roleId: newRoleId,
            role: roleName
          } : member;
        });

        setData(updatedData);
        setSelectedMembers(new Array(updatedData.length).fill(false));
        setSelectAll(false);
      } else if (selectedMemberForRoleSwap) {
        // Single member update
        await updateProjectMemberRole(
          Number(projectId),
          newRoleId,
          selectedMemberForRoleSwap.memberId || undefined,
          selectedMemberForRoleSwap.groupId || undefined
        );

        // Update local state for single member
        const updatedData = data.map(member => {
          const isMatch = member.memberId === selectedMemberForRoleSwap.memberId &&
            member.groupId === selectedMemberForRoleSwap.groupId;


          return isMatch
            ? {
              ...member,
              roleId: newRoleId,
              role: roleName
            }
            : member;
        });

        setData(updatedData);
      }

      handleRoleSwapClose();
    } catch (error) {
      console.error('Failed to update role:', error);
      throw error; // Re-throw so RoleSwap component can handle the error
    }
  };

  const multipleSelected = () => {
    return selectedMembers.filter(selected => selected).length > 1;
  };

  const columns: Column<ProjectMembersDto>[] = [
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (row: ProjectMembersDto, index: number) => (
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
      cell: (row: ProjectMembersDto) => (
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
      cell: (row: ProjectMembersDto, index: number) => (
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
        onRoleUpdate={handleRoleUpdate}
        currentMember={selectedMemberForRoleSwap}
        projectId={projectId}
        selectedMembers={multipleSelected() ? data.filter((_, index) => selectedMembers[index]) : undefined}
        roles={roles}
      />
    </div>
  );
};

export default MembersTable;