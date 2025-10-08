// import React, { FC, useState } from 'react';
// import GenericTable from '../../GenericTable';
// import { useLanguage } from "@/app/contexts/Language";
// import { Column, ProjectPermissionsTable } from '../../../types/types';

// interface PermissionTableProps {
//   data: ProjectPermissionsTable[];
// }

// const ProjectManagementTable: FC<PermissionTableProps> = ({ data: initialData }) => {
//     const { t } = useLanguage();
//     const [data, setData] = useState<ProjectPermissionsTable[]>(initialData);
//     const [selectedMembers, setSelectedMembers] = useState<boolean[]>(new Array(initialData.length).fill(false));
//     const [selectAll, setSelectAll] = useState<boolean>(false);

//     const handleSelectAll = () => {
//     const newSelection = !selectAll;
//     setSelectAll(newSelection);
//     setSelectedMembers(new Array(data.length).fill(newSelection));
//   };

//   const handleCheckboxChange = (index: number) => {
//     const newSelection = [...selectedMembers];
//     newSelection[index] = !newSelection[index];
//     setSelectedMembers(newSelection);

//     if (newSelection.every(Boolean)) {
//       setSelectAll(true);
//     } else {
//       setSelectAll(false);
//     }
//   };

//   const multipleSelected = () => {
//     return selectedMembers.filter(selected => selected).length > 1;
//   };

//     const columns: Column<ProjectPermissionsTable>[] = [
//         {
//           header: "Project Management",
//           data: "role",
//         },
//         {
//           header: "Description",
//           data: "description",
//           sortable: false,
//         },
//         {
//           header: (
//             <input
//               type="checkbox"
//               className="checkbox"
//               checked={selectAll}
//               onChange={handleSelectAll}
//             />
//           ),
//           cell: (row: ProjectPermissionsTable, index: number) => (
//                 <input
//                 type="checkbox"
//                 className="checkbox"
//                 checked={selectedMembers[index]}
//                 onChange={() => handleCheckboxChange(index)}
//                 />
//           ),
//           sortable: false,
//         },
//       ];

//     return (
//         <div className="">
//             <GenericTable
//                 columns={columns}
//                 data={data}
//                 // enablePagination
//                 // rowsPerPage={5}
//             />
//         </div>
//     );
// };

// export default ProjectManagementTable;

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

  return (
    <div className={className}>
      <GenericTable
        columns={projectColumns}
        data={projRows}
      />
    </div>
  );
};

export default RoleManagementTable;