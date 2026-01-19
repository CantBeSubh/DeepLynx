"use client";

import React from "react";
import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";

type Props = {
  isOpen: boolean;
  tagName: string;
  onClose: () => void;
  onConfirm: () => void;
  loading?: boolean;
};

const ConfirmArchiveTagModal: React.FC<Props> = ({
  isOpen,
  tagName,
  onClose,
  onConfirm,
  loading = false,
}) => {
  return (
    <dialog className={`modal ${isOpen ? "modal-open" : ""}`}>
      <div className="modal-box max-w-sm">
        <div className="flex items-start gap-3">
          <ExclamationTriangleIcon className="w-8 h-8 text-warning" />
          <div>
            <h3 className="font-bold text-lg">Archive Tag</h3>
            <p className="text-sm text-base-content/70 mt-1">
              Are you sure you want to archive{" "}
              <span className="font-semibold">{tagName}</span>?
              <br />
              This tag will no longer be available for new usage, but it can be
              restored later.
            </p>
          </div>
        </div>

        <div className="modal-action mt-6">
          <button
            className="btn btn-ghost"
            disabled={loading}
            onClick={onClose}
          >
            Cancel
          </button>

          <button
            className="btn btn-warning"
            disabled={loading}
            onClick={onConfirm}
          >
            {loading ? (
              <span className="loading loading-spinner loading-sm"></span>
            ) : (
              "Archive"
            )}
          </button>
        </div>
      </div>

      <div className="modal-backdrop" onClick={onClose} />
    </dialog>
  );
};

export default ConfirmArchiveTagModal;
