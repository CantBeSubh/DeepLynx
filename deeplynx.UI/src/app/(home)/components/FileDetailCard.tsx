"use client";
import React from "react";
import { ExistingFile } from "@/app/(home)/types/upload";

type Props = {
  needsTarget: boolean;
  selectedTarget: ExistingFile | null;
  className?: string;
};

export default function FileDetailsCard({
  needsTarget,
  selectedTarget,
  className = "",
}: Props) {
  if (!needsTarget) return null;

  return (
    <div className={`card card-border ${className}`}>
      <div className="card-body">
        <div className="flex justify-between items-center card-title w-full">
          <h2>
            {selectedTarget
              ? selectedTarget.alias || selectedTarget.name
              : "File details"}
          </h2>
          {selectedTarget?.timeSeries && (
            <span className="badge badge-info">Time series</span>
          )}
        </div>

        {!selectedTarget ? (
          <p className="text-sm opacity-70">
            Select an existing file from the dropdown.
          </p>
        ) : (
          <>
            <p className="text-sm opacity-90">{selectedTarget.description}</p>

            <div className="mt-3 grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm">
              <div>
                <span className="opacity-60">File name:</span>{" "}
                {selectedTarget.name}
              </div>
              <div>
                <span className="opacity-60">Alias:</span>{" "}
                {selectedTarget.alias}
              </div>
              <div>
                <span className="opacity-60">Last update:</span>{" "}
                {selectedTarget.lastUpdate}
              </div>
              <div>
                <span className="opacity-60">Updated by:</span>{" "}
                {selectedTarget.updatedBy}
              </div>
              <div className="sm:col-span-2">
                <span className="opacity-60">Data source:</span>{" "}
                {selectedTarget.dataSource}
              </div>
              <div className="sm:col-span-2">
                <span className="opacity-60">Properties sources:</span>{" "}
                {selectedTarget.propertiesSources}
              </div>
            </div>

            <div className="mt-3 flex flex-wrap gap-2">
              {selectedTarget.tags?.map((tag) => (
                <span key={tag} className="badge badge-outline">
                  {tag}
                </span>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
