// src/app/(home)/organization_management/groups/GroupArchiveModal.tsx"use client";

import React from "react";
import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";

interface GroupArchiveModalProps {
  isOpen: boolean;
  groupNames: string[];
  totalMembers?: number;
  onClose: () => void;
  onConfirm: () => void;
  loading?: boolean;
}

const GroupArchiveModal: React.FC<GroupArchiveModalProps> = ({
  isOpen,
  groupNames,
  totalMembers,
  onClose,
  onConfirm,
  loading = false,
}) => {
  if (!isOpen) return null;

  const isSingle = groupNames.length === 1;

  return (
    <div className="modal modal-open">
      <div className="modal-box max-w-lg">
        {/* Header */}
        <div className="flex items-start gap-3 mb-4">
          <div className="mt-1">
            <ExclamationTriangleIcon className="w-8 h-8 text-warning" />
          </div>
          <div>
            <h3 className="font-bold text-xl mb-1">
              {isSingle ? "Archive this group?" : "Archive selected groups?"}
            </h3>
            <p className="text-sm text-base-content/70">
              {isSingle
                ? "This group will be archived. Members will be removed from this group but will remain in the organization."
                : "The selected groups will be archived. Members will be removed from those groups but will remain in the organization."}
            </p>
          </div>
        </div>

        {/* Group summary */}
        <div className="bg-base-200 rounded-lg p-3 mb-4 text-sm">
          {isSingle ? (
            <>
              <div className="font-semibold mb-1">{groupNames[0]}</div>
              {typeof totalMembers === "number" && (
                <div className="text-base-content/70">
                  {totalMembers} member{totalMembers === 1 ? "" : "s"} in this
                  group
                </div>
              )}
            </>
          ) : (
            <>
              <div className="font-semibold mb-1">
                {groupNames.length} groups selected
              </div>
              <ul className="list-disc list-inside text-base-content/70 space-y-0.5 max-h-24 overflow-y-auto">
                {groupNames.slice(0, 4).map((name) => (
                  <li key={name}>{name}</li>
                ))}
                {groupNames.length > 4 && (
                  <li className="italic">+{groupNames.length - 4} more…</li>
                )}
              </ul>
            </>
          )}
        </div>

        {/* Warning blurb */}
        <div className="alert alert-warning mb-4">
          <ExclamationTriangleIcon className="w-5 h-5" />
          <div>
            <h4 className="font-semibold text-sm">What happens next?</h4>
            <p className="text-xs">
              The group will no longer appear in group lists. Users will not
              lose access to the organization itself, only this group
              association.
            </p>
          </div>
        </div>

        {/* Actions */}
        <div className="modal-action">
          <button
            className="btn btn-ghost"
            onClick={onClose}
            disabled={loading}
          >
            Cancel
          </button>
          <button
            className={`btn btn-error gap-2 ${loading ? "btn-disabled" : ""}`}
            onClick={onConfirm}
            disabled={loading}
          >
            {loading ? (
              <span className="loading loading-spinner loading-sm" />
            ) : null}
            Archive {isSingle ? "Group" : "Groups"}
          </button>
        </div>
      </div>

      {/* Backdrop */}
      <div className="modal-backdrop" onClick={onClose} />
    </div>
  );
};

export default GroupArchiveModal;
