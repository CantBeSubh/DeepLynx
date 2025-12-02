// src/app/(home)/organization_management/users/InviteUserModal.tsx

import React from "react";
import { EnvelopeIcon, XMarkIcon } from "@heroicons/react/24/outline";
import {
  ProjectResponseDto,
  RoleResponseDto,
} from "../../types/responseDTOs";

/* -------------------------------------------------------------------------- */
/*                            Invite User Dialog UI                           */
/* -------------------------------------------------------------------------- */

interface InviteUserModalProps {
  isOpen: boolean;
  inviteEmail: string;
  selectedProjectId: string;
  selectedRoleId: string;
  availableProjects: ProjectResponseDto[];
  availableRoles: RoleResponseDto[];
  modalLoading: boolean;

  onClose: () => void;
  onInvite: () => void;
  onChangeEmail: (value: string) => void;
  onChangeProject: (value: string) => void;
  onChangeRole: (value: string) => void;
}

const InviteUserModal: React.FC<InviteUserModalProps> = ({
  isOpen,
  inviteEmail,
  selectedProjectId,
  selectedRoleId,
  availableProjects,
  availableRoles,
  modalLoading,
  onClose,
  onInvite,
  onChangeEmail,
  onChangeProject,
  onChangeRole,
}) => {
  if (!isOpen) return null;

  const inviteDisabled =
    !inviteEmail || (selectedProjectId && !selectedRoleId) || modalLoading;

  return (
    <div className="modal modal-open">
      <div className="modal-box max-w-2xl">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <h3 className="font-bold text-2xl">Invite User to Organization</h3>
          <button
            className="btn btn-sm btn-circle btn-ghost"
            onClick={onClose}
            disabled={modalLoading}
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>

        {modalLoading ? (
          <div className="flex justify-center items-center py-12">
            <span className="loading loading-spinner loading-lg" />
          </div>
        ) : (
          <>
            <div className="space-y-4">
              {/* Email Input */}
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    Email Address <span className="text-error">*</span>
                  </span>
                </label>
                <input
                  type="email"
                  placeholder="user@example.com"
                  className="input input-bordered input-lg"
                  value={inviteEmail}
                  onChange={(e) => onChangeEmail(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && inviteEmail && !inviteDisabled) {
                      onInvite();
                    }
                  }}
                />
              </div>

              {/* <div className="divider">Optional Project Assignment</div> */}

              {/* Project Selector */}
              {/* <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold mr-2">
                    Assign to Project (Optional)
                  </span>
                </label>
                <select
                  className="select select-bordered select-lg"
                  value={selectedProjectId}
                  onChange={(e) => onChangeProject(e.target.value)}
                >
                  <option value="">No project (org access only)</option>
                  {availableProjects.map((project) => (
                    <option key={project.id} value={project.id}>
                      {project.name}
                    </option>
                  ))}
                </select>
              </div> */}

              {/* Role Selector (conditional) */}
              {selectedProjectId && (
                <div className="form-control">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Project Role <span className="text-error mr-2">*</span>
                    </span>
                  </label>
                  <select
                    className="select select-bordered select-lg"
                    value={selectedRoleId}
                    onChange={(e) => onChangeRole(e.target.value)}
                  >
                    <option value="">Select a role...</option>
                    {availableRoles.map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              {/* Info Alert */}
              <div className="alert alert-info">
                <EnvelopeIcon className="w-6 h-6" />
                <div>
                  <h4 className="font-semibold">Email Notification</h4>
                  <p className="text-sm">
                    An invitation email will be sent with instructions to join
                    the organization.
                    {selectedProjectId &&
                      " The user will be automatically assigned to the selected project with the specified role upon accepting."}
                  </p>
                </div>
              </div>
            </div>

            {/* Modal Actions */}
            <div className="modal-action">
              <button
                className="btn btn-ghost"
                onClick={onClose}
                disabled={modalLoading}
              >
                Cancel
              </button>
              <button
                className={`btn btn-primary gap-2 ${inviteDisabled ? "btn-disabled" : ""
                  }`}
                disabled={inviteDisabled}
                onClick={onInvite}
              >
                {modalLoading ? (
                  <span className="loading loading-spinner loading-sm" />
                ) : (
                  <EnvelopeIcon className="w-5 h-5" />
                )}
                Send Invitation
              </button>
            </div>
          </>
        )}
      </div>

      <div className="modal-backdrop" onClick={onClose} />
    </div>
  );
};

export default InviteUserModal;
