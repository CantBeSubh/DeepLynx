"use client";
import React from "react";

type Props = {
  multiple: boolean;
  disabled?: boolean;
  files: File[];
  onFilesChange: (files: File[]) => void;
};

export default function DropUpload({
  multiple,
  disabled,
  files,
  onFilesChange,
}: Props) {
  const handleInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const list = Array.from(e.target.files || []);
    onFilesChange(multiple ? list : list.slice(0, 1));
  };

  return (
    <div
      className={`p-4 rounded-xl border border-dashed ${
        disabled ? "opacity-50 pointer-events-none" : ""
      }`}
    >
      <input
        type="file"
        multiple={multiple}
        className="file-input file-input-bordered w-full"
        onChange={handleInput}
        disabled={disabled}
      />
      {files.length > 0 && (
        <ul className="mt-3 text-sm">
          {files.map((f, i) => (
            <li key={i} className="opacity-80">
              {f.name}{" "}
              <span className="opacity-50">
                ({Math.round(f.size / 1024)} KB)
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
