// src/app(home)/record/components/RelatedRecordsCard.tsx

"use client";

import React from "react";

export interface CardColumn<T extends object> {
  key: keyof T;
  label: string;
  /** Optional custom cell renderer */
  render?: (row: T) => React.ReactNode;
}

interface RelatedRecordsCardProps<T extends object> {
  title?: string;
  columns: CardColumn<T>[];
  rows: T[];
  /** Show leading index column like your original table */
  showIndex?: boolean;
}

function RelatedRecordsCard<T extends object>({
  title = "Related Records:",
  columns,
  rows,
  showIndex = true,
}: RelatedRecordsCardProps<T>) {
  return (
    <div className="card bg-base-100 shadow-md p-2">
      <h2 className="text-xl font-bold md-4 text-base-content">{title}</h2>
      <div className="card-body p-4">
        <div className="overflow-x-auto rounded-box border border-base-300 bg-base-100">
          <table className="table">
            <thead>
              <tr className="bg-base-200 text-base-content">
                {showIndex && <th></th>}
                {columns.map((col) => (
                  <th key={String(col.key)}>{col.label}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map((row, i) => (
                <tr key={i}>
                  {showIndex && <th>{i + 1}</th>}
                  {columns.map((col) => {
                    const raw = row[col.key];
                    const content = col.render
                      ? col.render(row)
                      : (raw as React.ReactNode);
                    return <td key={String(col.key)}>{content}</td>;
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default RelatedRecordsCard;
