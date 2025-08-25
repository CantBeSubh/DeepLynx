"use client";

import React, { useEffect, useState } from "react";

export default function NewFileUploadCard({
  defaultName = "",
}: {
  defaultName?: string;
}) {
  const [name, setName] = useState(defaultName);

  // put near the top of UploadCenterClient.tsx
  const fileBaseName = (filename: string) => filename.replace(/\.[^/.]+$/, "");
  // If you want to cut at the FIRST dot instead:
  // const fileBaseName = (filename: string) => filename.split(".")[0];

  useEffect(() => {
    setName(defaultName);
  }, [defaultName]);
  return (
    <div>
      <h4 className="px-2">{name}</h4>
      <div className="card card-border">
        <div className="card-body w-full space-y-4">
          {/* Row 1: Time Series toggle + Name input */}
          <div className="grid grid-cols-[auto,1fr] items-center gap-4">
            <div className="flex items-center">
              <span className="label-text mr-2">Time Series</span>
              <input type="checkbox" className="toggle toggle-secondary" />
              <label className="flex items-center gap-2 flex-1">
                <span className="label-text ml-4">Name</span>
                <input
                  type="text"
                  className="input input-sm w-full"
                  placeholder="metadata.a"
                  value={fileBaseName(name)}
                  onChange={(e) => setName(e.target.value)}
                />
              </label>
            </div>
          </div>

          {/* Row 2: Description textarea */}
          <div className="grid grid-cols-[auto,1fr] items-start gap-4">
            <div className="flex">
              <span className="label-text mr-2">Description</span>
              <textarea
                className="textarea textarea-bordered w-full"
                placeholder="Example: This file contains ..."
              ></textarea>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
