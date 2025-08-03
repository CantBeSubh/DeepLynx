import { PencilIcon } from "@heroicons/react/24/outline";
import React from "react";

interface PropertyRow {
  label: string;
  value: React.ReactNode;
  editable?: boolean;
  onEdit?: () => void;
}

interface PropertyTableProps {
  title?: string;
  rows: PropertyRow[];
  className?: string;
}

const PropertyTable: React.FC<PropertyTableProps> = ({
  title,
  rows,
  className,
}) => {
  return (
    <div className={`${className}`}>
      <div className="card bg-base-100 shadow-md p-2">
        {title && <h2 className="text-xl font-bold mb-4">{title}</h2>}
        <div className="card-body p-4">
          <div className="border border-base-200 rounded-lg overflow-hidden bg-white">
            {rows.map((row, index) => (
              <div
                key={index}
                className={`grid grid-cols-12 ${
                  index !== rows.length - 1 ? "border-b" : ""
                } border-base-200`}
              >
                <div className="col-span-4 p-3 font-medium text-info-content text-sm bg-base-50 border-r border-base-200">
                  {row.label}
                </div>
                <div className="col-span-7 p-3 text-sm">{row.value}</div>
                <div className="col-span-1 p-3 flex justify-center items-center">
                  {row.editable && <PencilIcon className="text-secondary" />}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PropertyTable;
