"use client";
import React from "react";

type Props = {
  files: File[];
  onRemoveAt: (idx: number) => void;
  onClear: () => void;
  onUpload: () => void;
};

export default function SelectedFilesCard({
  files,
  onRemoveAt,
  onClear,
  onUpload,
}: Props) {
  if (files.length === 0) return null;
  return (
    <div className="card card-border mt-4">
      <div className="card-body">
        <h2 className="card-title">Selected files</h2>
        {files.length === 0 ? (
          <p className="text-sm opacity-70">No files selected yet.</p>
        ) : (
          <>
            <ul className="space-y-2">
              {files.map((f, i) => (
                <li key={i} className="flex items-center justify-between gap-3">
                  <div className="truncate">
                    <b className="mr-2">{f.name}</b>
                    <span className="opacity-60 text-xs">
                      {Math.round(f.size / 1024)} KB
                    </span>
                  </div>
                  <button className="btn btn-xs" onClick={() => onRemoveAt(i)}>
                    Remove
                  </button>
                </li>
              ))}
            </ul>
            <div className="mt-4 flex gap-2">
              <button className="btn btn-ghost btn-sm" onClick={onClear}>
                Clear all
              </button>
              <button className="btn btn-secondary btn-sm" onClick={onUpload}>
                Upload
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
