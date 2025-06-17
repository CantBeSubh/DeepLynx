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
    <div className="">
      <table className="table w-full border-separate border-spacing-y-2">
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
                  <td colSpan={columns.length + 2} className="p-0 ">
                    <div
                      className={`overflow-hidden transition-all duration-100 ease-in-out ${
                        expandedIndex === index
                          ? "max-h-100 opacity-1000"
                          : "max-h-0 opacity-0"
                      }`}
                    >
                      <div className="card bg-base-200/40 transition-all duration-900 ease-in-out animate-fade-in p-4">
                        {renderExpandedContent(row, closeExpanded)}
                      </div>
                    </div>
                  </td>
                </tr>
              ) : (
                <tr className="bg-base-200/20 hover:bg-base-200/40 mb-2">
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
                    <button onClick={() => toggleRow(index)}>
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        strokeWidth={1.5}
                        stroke="currentColor"
                        className="size-6"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="m19.5 8.25-7.5 7.5-7.5-7.5"
                        />
                      </svg>
                    </button>
                  </td>
                </tr>
              )}
            </React.Fragment>
          ))}
        </tbody>
      </table>

      {/* Example pagination placeholder */}
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
