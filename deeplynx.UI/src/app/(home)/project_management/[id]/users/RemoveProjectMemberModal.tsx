"use client";

import React from "react";
import { ConfirmModalState } from "../../types/projectUsersTypes";

/* -------------------------------------------------------------------------- */
/*                        Remove Member Confirm Modal                         */
/* -------------------------------------------------------------------------- */

interface RemoveProjectMemberModalProps {
  confirmModal: ConfirmModalState;
  loading: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

const RemoveProjectMemberModal: React.FC<RemoveProjectMemberModalProps> = ({
  confirmModal,
  loading,
  onCancel,
  onConfirm,
}) => {
  if (!confirmModal.isOpen) return null;

  return (
    <dialog className="modal modal-open">
      <div className="modal-box">
        <h3 className="font-bold text-lg">Remove Member?</h3>
        <p className="py-4">
          Are you sure you want to remove{" "}
          <span className="font-semibold">{confirmModal.memberName}</span> from
          this project? They will lose access to this project.
        </p>
        <div className="modal-action">
          <button className="btn btn-ghost" onClick={onCancel}>
            Cancel
          </button>
          <button
            className="btn btn-error"
            disabled={loading}
            onClick={onConfirm}
          >
            {loading ? "Removing..." : "Remove"}
          </button>
        </div>
      </div>
    </dialog>
  );
};

export default RemoveProjectMemberModal;
