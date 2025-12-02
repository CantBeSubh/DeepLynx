"use client";

import React from "react";
import {
  UserResponseDto,
  GroupResponseDto,
  RoleResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { AddMemberModalState, MemberType } from "../../types/projectUsersTypes";

/* -------------------------------------------------------------------------- */
/*                           Add Member Modal Component                       */
/* -------------------------------------------------------------------------- */

interface AddProjectMemberModalProps {
  addModal: AddMemberModalState;
  roles: RoleResponseDto[];
  availableUsers: UserResponseDto[];
  availableGroups: GroupResponseDto[];
  selectedMemberId: string;
  selectedRoleId: string;
  modalLoading: boolean;
  onClose: () => void;
  onChangeMember: (value: string) => void;
  onChangeRole: (value: string) => void;
  onConfirm: () => void;
}

const AddProjectMemberModal: React.FC<AddProjectMemberModalProps> = ({
  addModal,
  roles,
  availableUsers,
  availableGroups,
  selectedMemberId,
  selectedRoleId,
  modalLoading,
  onClose,
  onChangeMember,
  onChangeRole,
  onConfirm,
}) => {
  if (!addModal.isOpen) return null;

  const labelForType: Record<MemberType, string> = {
    user: "user",
    group: "group",
  };

  return (
    <dialog className="modal modal-open">
      <div className="modal-box">
        <h3 className="font-bold text-lg">
          {addModal.memberType === "user"
            ? "Add User to Project"
            : "Add Group to Project"}
        </h3>
        <p className="py-2 text-sm text-base-content/70">
          Select an existing {addModal.memberType} and assign a role. A role is
          required to add them to the project.
        </p>

        {/* Member select */}
        <div className="form-control mt-4">
          <label className="label">
            <span className="label-text capitalize">
              {labelForType[addModal.memberType]}
            </span>
          </label>
          <select
            className="select select-bordered w-full"
            value={selectedMemberId}
            onChange={(e) => onChangeMember(e.target.value)}
            disabled={modalLoading}
          >
            <option value="">
              {addModal.memberType === "user"
                ? "Select a user"
                : "Select a group"}
            </option>

            {addModal.memberType === "user"
              ? availableUsers.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.name} {u.email ? `(${u.email})` : ""}
                  </option>
                ))
              : availableGroups.map((g) => (
                  <option key={g.id} value={g.id}>
                    {g.name}
                  </option>
                ))}
          </select>
        </div>

        {/* Role select */}
        <div className="form-control mt-4">
          <label className="label">
            <span className="label-text">Role</span>
          </label>
          <select
            className="select select-bordered w-full"
            value={selectedRoleId}
            onChange={(e) => onChangeRole(e.target.value)}
            disabled={modalLoading}
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
            onClick={onClose}
            disabled={modalLoading}
          >
            Cancel
          </button>
          <button
            className="btn btn-primary"
            onClick={onConfirm}
            disabled={modalLoading}
          >
            {modalLoading ? "Adding..." : "Add to Project"}
          </button>
        </div>
      </div>
    </dialog>
  );
};

export default AddProjectMemberModal;
