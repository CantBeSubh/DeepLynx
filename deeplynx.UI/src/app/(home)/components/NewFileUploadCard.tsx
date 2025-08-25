"use client";
import React from "react";

export default function NewFileUploadCard() {
  return (
    <div className="card card-border">
      <div className="card-body">
        <div className="flex items-center justify-between">
          <label className="label gap-2">
            <span className="label-text">Time Series</span>
            <input type="checkbox" className="toggle toggle-secondary" />
          </label>
          <label className="label gap-2">
            <span className="label-text">Name</span>
            <input
              type="text"
              className="input input-sm"
              placeholder="Optional alias"
            />
          </label>
        </div>
      </div>
    </div>
  );
}
