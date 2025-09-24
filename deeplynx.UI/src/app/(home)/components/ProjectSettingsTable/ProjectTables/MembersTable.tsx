import React, { FC, useState } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column, ProjectMembersTable } from '../../../types/types';
import RoleSwap from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/RoleSwap";
import { TrashIcon } from '@heroicons/react/24/outline';

interface MembersTableProps {
  data: ProjectMembersTable[];
}

const MembersTable: FC<MembersTableProps> = ({ data: initialData }) => {
  const { t } = useLanguage();
  const [data, setData] = useState<ProjectMembersTable[]>(initialData);
  const [addRoleSwap, setAddRoleSwap] = useState<boolean>(false);
  const [selectedMembers, setSelectedMembers] = useState<boolean[]>(new Array(initialData.length).fill(false));
  const [selectAll, setSelectAll] = useState<boolean>(false);

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
            {multipleSelected() && (
                <button className="btn" onClick={() => setAddRoleSwap(true)}>
                    {t.translations.ROLE}
                </button>
            )}
        </div>
      ),
      cell: (row: ProjectMembersTable) => (
        <div className="flex">
            <button className="btn"
                onClick={() => setAddRoleSwap(true)}>
                {row.role}
                {/* {t.translations.ROLE} */}
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

        <RoleSwap
            isOpen={addRoleSwap}
            onClose={() => setAddRoleSwap(false)}
        />
    </div>
  );
};

export default MembersTable;