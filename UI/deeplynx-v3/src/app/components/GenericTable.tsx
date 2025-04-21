import React from "react";

type Column = {
  header: string;
  accessor: string;
  cell?: (row: any) => React.ReactNode;
};

type GenericTableProps = {
  columns: Column[];
  data: any[];
};

const GenericTable: React.FC<GenericTableProps> = ({ columns, data }) => {
  return (
    <div className="overflow-x-auto">
      <table className="table">
        {/* head */}
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th key={index}>{column.header}</th>
            ))}
            <th></th>
          </tr>
        </thead>
        <tbody>
          {data.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {columns.map((column, colIndex) => (
                <td key={colIndex} className="text-primary-content">
                  {column.cell ? column.cell(row) : row[column.accessor]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default GenericTable;
