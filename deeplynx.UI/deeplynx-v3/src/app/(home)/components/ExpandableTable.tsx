import { ArrowDownIcon, ChevronDownIcon } from "@heroicons/react/24/outline";
import React, { useState, ReactNode } from "react";

interface ExpandableTableProps<T> {
  data: T[];
  columns: {
    header: string;
    data: (row: T) => ReactNode;
  }[];
  renderExpandedContent: (row: T, onClose: () => void) => ReactNode;
  onExplore: (row: T) => void;
}

export function ExpandableTable<T>({
  data,
  columns,
  renderExpandedContent,
  onExplore,
}: ExpandableTableProps<T>) {
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);

  const toggleRow = (index: number) => {
    setExpandedIndex(expandedIndex === index ? null : index);
  };

  const closeExpanded = () => setExpandedIndex(null);

  return (
    <div>
      <table className="table w-full border-separate p-2 border-spacing-y-2 shadow">
        {expandedIndex === null && (
          <thead>
            <tr>
              {columns.map((col, i) => (
                <th key={i}>{col.header}</th>
              ))}
              <th></th>
            </tr>
          </thead>
        )}

        <tbody>
          {data.map((row, index) => (
            <React.Fragment key={index}>
              {expandedIndex === index ? (
                <tr>
                  <td colSpan={columns.length + 2} className="p-0">
                    <div className="overflow-hidden transition-all duration-500 ease-in-out max-h-[1000px] opacity-100">
                      <div className="card bg-base-200/40 p-6 rounded-box shadow">
                        {renderExpandedContent(row, closeExpanded)}
                      </div>
                    </div>
                  </td>
                </tr>
              ) : (
                <tr className="bg-base-200/20 hover:bg-base-200/40 rounded-lg overflow-hidden">
                  {columns.map((col, i) => (
                    <td key={i}>{col.data(row)}</td>
                  ))}
                  <td>
                    <button
                      className="btn btn-sm btn-outline btn-secondary hover:text-primary-content mr-3"
                      onClick={() => onExplore(row)}
                    >
                      Explore
                    </button>
                  </td>
                  <td>
                    <button
                      onClick={() => toggleRow(index)}
                      aria-label="Expand row"
                    >
                      <ChevronDownIcon className="size-6" />
                    </button>
                  </td>
                </tr>
              )}
            </React.Fragment>
          ))}
        </tbody>
      </table>

      <div className="flex justify-center mt-4">
        <div className="join">
          <button className="join-item btn">«</button>
          {[1, 2, 3].map((page) => (
            <button key={page} className="join-item btn">
              {page}
            </button>
          ))}
          <button className="join-item btn">»</button>
        </div>
      </div>
    </div>
  );
}
