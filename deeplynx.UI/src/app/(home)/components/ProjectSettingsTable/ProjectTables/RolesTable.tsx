import React, { FC, useState } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column, MyRolesTable } from '../../../types/types';
import { TrashIcon, PencilIcon } from '@heroicons/react/24/outline';
import { useRouter } from "next/navigation";

interface RolesTableProps {
  data: MyRolesTable[];
  id: string | null | undefined;
}

const RolesTable: FC<RolesTableProps> = ({ data: initialData, id }) => {
  const { t } = useLanguage();
  const [data, setData] = useState<MyRolesTable[]>(initialData);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>(new Array(initialData.length).fill(false));
  const [selectAll, setSelectAll] = useState<boolean>(false);
  const [handleEdit, setHandleEdit] = useState<boolean>(false);
  const router = useRouter();

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

  const handleDelete = (index: number) => {
    const newData = data.filter((_, i) => i !== index);
    setData(newData);
    setSelectedMembers(new Array(newData.length).fill(false));
    setSelectAll(false);
  };

  const handleDeleteSelected = () => {
    const newData = data.filter((_, index) => !selectedMembers[index]);
    setData(newData);
    setSelectedMembers(new Array(newData.length).fill(false));
    setSelectAll(false);
  };

  const multipleSelected = () => {
    return selectedMembers.filter(selected => selected).length > 1;
  };

  const columns: Column<MyRolesTable>[] = [
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (row: MyRolesTable, index: number) => (
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
      data: "role",
    },
    {
      header: "Description",
      data: "description",
      sortable: false,
    },
    {
      header: "",
      cell: (row: MyRolesTable) => (
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
      cell: (row: MyRolesTable, index: number) => (
        <div className="flex">
          <button onClick={() => handleDelete(index)}>
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