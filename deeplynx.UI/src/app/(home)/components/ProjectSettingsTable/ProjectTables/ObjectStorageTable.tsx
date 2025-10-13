import React, { FC } from 'react';
import GenericTable from '../../GenericTable';
import { Column, MySearchsTable } from '../../../types/types';
import { TrashIcon } from '@heroicons/react/24/outline';

interface ObjectStorageTableProps {
  data: MySearchsTable[];
}

const ObjectStorageTable: FC<ObjectStorageTableProps> = ({ data }) => {
  const columns: Column<MySearchsTable>[] = [
    {
        header: "Name",
        data: "name",
    },
    {
        header: "Email",
        sortable: false,
    },
    {
        header: "",
        cell: () => (
        <div className="flex justify-end">
            <TrashIcon className="size-6 text-red-500" />
        </div>
        ),
        sortable: false,
    },
  ];

  return (
    <GenericTable
        columns={columns}
        data={data}
        enablePagination
        rowsPerPage={5}
    />
  );
};

export default ObjectStorageTable;