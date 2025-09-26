import React, { FC, useState } from 'react';
import GenericTable from '../../GenericTable';
import EditRole from '@/app/(home)/components/ProjectSettingsTable/ProjectModals/EditRole'
import { useLanguage } from "@/app/contexts/Language";
import { Column, UserPermissionsTable } from '../../../types/types';
import { TrashIcon, PencilIcon } from '@heroicons/react/24/outline';

interface UserPermsTableProps {
  data: UserPermissionsTable[];
}

const UserManagementTable: FC<UserPermsTableProps> = ({ data: initialData }) => {
    const { t } = useLanguage();
    const [data, setData] = useState<UserPermissionsTable[]>(initialData);
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

  const multipleSelected = () => {
    return selectedMembers.filter(selected => selected).length > 1;
  };

    const columns: Column<UserPermissionsTable>[] = [
        {
          header: "User Mangement",
          data: "role",
        },
        {
          header: "Description",
          data: "description",
          sortable: false,
        },
        {
          header: (
            <input
              type="checkbox"
              className="checkbox"
              checked={selectAll}
              onChange={handleSelectAll}
            />
          ),
          cell: (row: UserPermissionsTable, index: number) => (
                <input
                type="checkbox"
                className="checkbox"
                checked={selectedMembers[index]}
                onChange={() => handleCheckboxChange(index)}
                />
          ),
          sortable: false,
        },
      ];

    return (
        <div>
            <GenericTable
                columns={columns}
                data={data}
                // enablePagination
                // rowsPerPage={5}
            />
        </div>
    );
};

export default UserManagementTable;