"use client";

import React from "react";
import { EditRoleModalState } from "../../types/projectUsersTypes";
import { RoleResponseDto } from "@/app/(home)/types/responseDTOs";

/* -------------------------------------------------------------------------- */
/*                           Edit Member Role Modal                           */
/* -------------------------------------------------------------------------- */

interface EditProjectMemberRoleModalProps {
  editRoleModal: EditRoleModalState;
  roles: RoleResponseDto[];
  loading: boolean;
  selectedRoleId: string;
  onChangeRole: (value: string) => void;
  onCancel: () => void;
  onSave: () => void;
}

const EditProjectMemberRoleModal: React.FC<EditProjectMemberRoleModalProps> = ({
  editRoleModal,
  roles,
  loading,
  selectedRoleId,
  onChangeRole,
  onCancel,
  onSave,
}) => {
  if (!editRoleModal.isOpen) return null;

  return (
    <dialog className="modal modal-open">
      <div className="modal-box">
        <h3 className="font-bold text-lg">Edit Member Role</h3>
        <p className="py-2 text-sm text-base-content/70">
          Change the role for{" "}
          <span className="font-semibold">{editRoleModal.memberName}</span> in
          this project.
        </p>

        <div className="form-control mt-4">
          <label className="label">
            <span className="label-text">Role</span>
          </label>
          <select
            className="select select-bordered w-full"
            value={selectedRoleId}
            onChange={(e) => onChangeRole(e.target.value)}
            disabled={loading}
          >
            <option value="">Select a role</option>
            {roles.map((r) => (
              <option key={r.id} value={r.id}>
                {r.name}
              </option>
            ))}
          </select>
        </div>

        <div className="modal-action">
          <button
            className="btn btn-ghost"
            onClick={onCancel}
            disabled={loading}
          >
            Cancel
          </button>
          <button
            className="btn btn-primary"
            onClick={onSave}
            disabled={loading}
          >
            {loading ? "Saving..." : "Save"}
          </button>
        </div>
      </div>
    </dialog>
  );
};

export default EditProjectMemberRoleModal;
