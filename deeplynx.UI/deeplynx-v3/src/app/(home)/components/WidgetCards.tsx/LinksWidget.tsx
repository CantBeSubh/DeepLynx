import React from "react";
import {AdjustmentsHorizontalIcon, FolderIcon, DocumentDuplicateIcon, ArrowTrendingUpIcon} from "@heroicons/react/24/outline";

const LinksWidget = () => {
  return (
    <div className="card-body">
      <div className="flex justify-between items-center">
        <h2 className="card-title">
          Links
        </h2>
        <button>
          <PlusIcon />
        </button>
      </div>
      <div className="flex justify-between p-4">
        <div className="flex flex-col items-center">
          <AdjustmentsHorizontalIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary">Roles</button>
        </div>
        <div className="flex flex-col items-center">
          <FolderIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary flex flex-col items-center">
            File Explorer
          </button>
        </div>
        <div className="flex flex-col items-center">
          <DocumentDuplicateIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary flex flex-col items-center">
            Reports
          </button>
        </div>
        <div className="flex flex-col items-center">
          <ArrowTrendingUpIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary ">Trends</button>
        </div>
      </div>
    </div>
  );
};

const PlusIcon = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="oklch(44.08% 0.141 255.19)"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="white"
    className="w-8 h-8 rounded-full"
  >
    <circle
        cx="12"
        cy="12"
        r="12"
        fill="btn-secondary"
    />
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M12 8v8m4-4h-8"
    />
  </svg>
);

export default LinksWidget;
