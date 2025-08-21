import {
  CheckCircleIcon,
  PencilIcon,
  XCircleIcon,
} from "@heroicons/react/24/outline";
import React, { useState } from "react";

interface PropertyRow {
  label: string;
  value: React.ReactNode;
  editable?: boolean;
  onEdit?: (newValue: string) => void;
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
  const [editingIdex, setEditingIndex] = useState<number | null>(null);
  const [editValue, setEditValue] = useState<string>("");

  const handleEdit = (index: number, currentValue: string) => {
    setEditingIndex(index);
    setEditValue(currentValue);
  };

  const handleSave = (row: PropertyRow) => {
    row.onEdit?.(editValue);
    setEditingIndex(null);
  };

  const handleCancel = () => {
    setEditingIndex(null);
    setEditValue("");
  };

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
                <div className="col-span-7 p-3 text-sm break-words">
                  {editingIdex === index ? (
                    <input
                      type="text"
                      value={editValue}
                      onChange={(e) => setEditValue(e.target.value)}
                      className="input input-sm input-bordered w-full"
                    ></input>
                  ) : (
                    <div className="break-words">{row.value}</div>
                  )}
                </div>
                <div className="col-span-1 p-3 flex justify-center items-center">
                  {row.editable && editingIdex !== index && (
                    <PencilIcon
                      className="text-secondary size-6 cursor-pointer"
                      onClick={() => handleEdit(index, String(row.value))}
                    />
                  )}
                  {editingIdex === index && (
                    <>
                      <button className="">
                        <CheckCircleIcon
                          className="text-success size-6 cursor-pointer"
                          onClick={() => handleSave(row)}
                        />
                      </button>
                      <button>
                        <XCircleIcon
                          className="text-error size-6 cursor-pointer"
                          onClick={handleCancel}
                        />
                      </button>
                    </>
                  )}
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