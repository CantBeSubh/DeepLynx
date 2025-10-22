import React, { FC, useState, useEffect } from 'react';
import GenericTable from '../../GenericTable';
import { useLanguage } from "@/app/contexts/Language";
import { Column } from '../../../types/types';
import { RoleResponseDto, PermissionResponseDto } from "../../../types/types";

interface RoleManagementTableProps {
  projectData: PermissionResponseDto[];
  className?: string;
}

const RoleManagementTable: FC<RoleManagementTableProps> = ({
  projectData,
  className,
}) => {
  const { t } = useLanguage();
import { PermissionResponseDto } from '@/app/(home)/types/responseDTOs';
interface PermissionTableProps {
  data: PermissionResponseDto[];
}

const ProjectManagementTable: FC<PermissionTableProps> = ({ data: initialData }) => {
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

  const handleProjSelectAll = () => {
    const next = !projSelectAll;
    setProjSelectAll(next);
    setProjSelected(new Array(projRows.length).fill(next));
  };

  const handleProjCheckbox = (index: number) => {
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
          onChange={handleProjSelectAll}
        />
      ),
      cell: (_row, i) => (
        <input
          type="checkbox"
          className="checkbox"
          checked={projSelected[i]}
          onChange={() => handleProjCheckbox(i)}
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
              onChange={handleProjSelectAll}
            />
          ),
          cell: (row: PermissionResponseDto, index: number) => (
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
};

export default RoleManagementTable;