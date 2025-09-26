import React, { FC, useState } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column, ProjectPermissionsTable } from '../../../types/types';

interface PermissionTableProps {
  data: ProjectPermissionsTable[];
}

const ProjectManagementTable: FC<PermissionTableProps> = ({ data: initialData }) => {
    const { t } = useLanguage();
    const [data, setData] = useState<ProjectPermissionsTable[]>(initialData);
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

    const columns: Column<ProjectPermissionsTable>[] = [
        {
          header: "Project Management",
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
          cell: (row: ProjectPermissionsTable, index: number) => (
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

export default ProjectManagementTable;