import React, { FC, useState } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column, ProjectMembersTable } from '../../../types/types';
import RoleSwap from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/RoleSwap";
import { TrashIcon } from '@heroicons/react/24/outline';

interface MembersTableProps {
  data: ProjectMembersTable[];
}

const MembersTable: FC<MembersTableProps> = ({ data }) => {
  const { t } = useLanguage();
  const [addRoleSwap, setAddRoleSwap] = useState(false);

  const [selectedMembers, setSelectedMembers] = useState<boolean[]>(new Array(data.length).fill(false));
  const [selectAll, setSelectAll] = useState(false);

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
      sortable: false,
    },
    {
      header: (
        <div className="flex">
          <button className="btn"
            onClick={() => setAddRoleSwap(true)}>
            {t.translations.ROLE}
        </button>
        </div>
      ),
      cell: () => (
        <div className="flex">
          <button className="btn"
            onClick={() => setAddRoleSwap(true)}>
            {t.translations.ROLE}
        </button>
        </div>
      ),
      sortable: false,
    },
    {
      header: (
        <div className="flex">
          <TrashIcon className="size-6 text-red-500" />
        </div>
      ),
      cell: () => (
        <div className="flex">
            <button className="">
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

        <RoleSwap
            isOpen={addRoleSwap}
            onClose={() => setAddRoleSwap(false)}
        />
    </div>
  );
};

export default MembersTable;