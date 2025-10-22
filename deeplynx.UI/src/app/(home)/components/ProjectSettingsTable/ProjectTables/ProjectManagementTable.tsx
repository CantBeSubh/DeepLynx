import React, { FC, useState, useEffect } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column } from '../../../types/types';
import { PermissionResponseDto, RoleResponseDto } from '@/app/(home)/types/responseDTOs';

interface RoleManagementTableProps {
  projectData: PermissionResponseDto[];
  initialData: PermissionResponseDto[];
}
// interface PermissionTableProps {
//   data: PermissionResponseDto[];
// }

const RoleManagementTable: FC<RoleManagementTableProps> = ({
  initialData,
  projectData,
}) => {
    const { t } = useLanguage();
    const [data, setData] = useState<PermissionResponseDto[]>(initialData);
    const [selectedMembers, setSelectedMembers] = useState<boolean[]>(new Array(initialData.length).fill(false));
    const [selectAll, setSelectAll] = useState<boolean>(false);

  // ===== Project Management table state =====
  const [projRows, setProjRows] = useState<PermissionResponseDto[]>(projectData);
  const [projSelected, setProjSelected] = useState<boolean[]>(
    new Array(projectData.length).fill(false)
  );
  const [projSelectAll, setProjSelectAll] = useState(false);

  useEffect(() => {
    setProjRows(projectData);
    setProjSelected(new Array(projectData.length).fill(false));
    setProjSelectAll(false);
  }, [projectData]);

  const handleSelectAll = () => {
    const next = !projSelectAll;
    setProjSelectAll(next);
    setProjSelected(new Array(projRows.length).fill(next));
  };

  const handleCheckbox = (index: number) => {
    const next = [...projSelected];
    next[index] = !next[index];
    setProjSelected(next);
    setProjSelectAll(next.every(Boolean));
  };

  const projectColumns: Column<PermissionResponseDto>[] = [
    {
      header: t?.translations?.PROJECT_MANAGEMENT ?? "Project Management",
      data: "name",
    },
    {
      header: t?.translations?.DESCRIPTION ?? "Description",
      data: "description",
      sortable: false,
    },
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={projSelectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (_row, i) => (
        <input
          type="checkbox"
          className="checkbox"
          checked={projSelected[i]}
          onChange={() => handleCheckbox(i)}
        />
      ),
      sortable: false,
    },
  ];

  const columns: Column<PermissionResponseDto>[] = [
      {
        header: "Project Management",
        data: "action",
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
        cell: (row: PermissionResponseDto, index: number) => (
              <input
              type="checkbox"
              className="checkbox"
              checked={selectedMembers[index]}
              onChange={() => handleCheckbox(index)}
              />
        ),
        sortable: false,
      },
    ];

  return (
      <div>
          <GenericTable
              columns={columns}
              data={initialData}
          />
      </div>
  );
};

export default RoleManagementTable;