import React, { FC, useState, useEffect } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column, RoleResponseDto } from '../../../types/types';
import { TrashIcon, PencilIcon } from '@heroicons/react/24/outline';
import { useRouter } from "next/navigation";
import { getAllRoles, deleteRole } from "@/app/lib/role_services.client";

interface RolesTableProps {
  data: RoleResponseDto[];
  id: string | null | undefined;
}

const RolesTable: FC<RolesTableProps> = ({ data: initialData, id }) => {
  const { t } = useLanguage();
  const [data, setData] = useState<RoleResponseDto[]>(initialData);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>(
    new Array(initialData.length).fill(false));
  const [selectAll, setSelectAll] = useState<boolean>(false);
  const [handleEdit, setHandleEdit] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();

  //Keeping selection array in sync
  useEffect(() => {
    setSelectedMembers((prev) => {
      if (prev.length !== data.length) {
        return new Array(data.length).fill(false);
      }
      return prev;
    });
  }, [data.length]);

  //Fetching roles when component mounts or Id changes
  useEffect(() => {
    let cancelled = false;

    const fetchRoles = async () => {
      // parse project id from string prop, if present
      const projectId = id ? Number(id) : undefined;
      if (id && Number.isNaN(projectId)) {
        setError("Invalid project id.");
        return;
      }

      setLoading(true);
      setError(null);
      try {
        const roles: RoleResponseDto[] = await getAllRoles(projectId, undefined, true);
        if (!cancelled) {
          setData(roles ?? []);
          setSelectAll(false);
          setSelectedMembers(new Array((roles ?? []).length).fill(false));
        }
      } catch (e) {
        if (!cancelled) setError("Failed to load roles.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    fetchRoles();
    return () => {
      cancelled = true;
    };
  }, [id]);

  //Function for selecting members
  const handleSelectAll = () => {
    const newSelection = !selectAll;
    setSelectAll(newSelection);
    setSelectedMembers(new Array(data.length).fill(newSelection));
  };

  //Function for checkboxes
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

  // Function for deleting a single role
  const handleDelete = async (roleToDelete: RoleResponseDto, index: number) => {
    try {
      await deleteRole(roleToDelete.id);
      const newData = data.filter((_, i) => i !== index);
      setData(newData);
      setSelectedMembers(new Array(newData.length).fill(false));
      setSelectAll(false);
    } catch (error) {
      console.error('Failed to delete role:', error);
    }
  };

  // Function for deleting selected roles
  const handleDeleteSelected = async () => {
    const rolesToDelete = data.filter((_, index) => selectedMembers[index]);

    try {
      // Delete all selected roles
      await Promise.all(
        rolesToDelete.map(role =>
          deleteRole(role.id)
        )
      );

      const newData = data.filter((_, index) => !selectedMembers[index]);
      setData(newData);
      setSelectedMembers(new Array(newData.length).fill(false));
      setSelectAll(false);
    } catch (error) {
      console.error('Failed to delete selected roles:', error);
    }
  };

  //Function for selecting multiple roles
  const multipleSelected = () => {
    return selectedMembers.filter(selected => selected).length > 1;
  };

  //Table for displaying project roles
  const columns: Column<RoleResponseDto>[] = [
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (row: RoleResponseDto, index: number) => (
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
      header: "Role",
      data: "name",
    },
    {
      header: "Description",
      data: "description",
      sortable: false,
    },
    {
      header: "",
      cell: (row: RoleResponseDto) => (
        <div className="flex">
          <button onClick={() => router.push(`/project/${id}/project_settings/project_roles?roleId=${row.id}`)}>
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
      cell: (row: RoleResponseDto, index: number) => (
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
      // rowsPerPage={5}
      />
    </div>
  );
};

export default RolesTable;