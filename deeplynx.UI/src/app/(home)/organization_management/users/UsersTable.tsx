// src/app/(home)/organization_management/users/UsersTable.tsx
"use client";

import React, { useState, useEffect } from "react";
import {
  UserIcon,
  UserGroupIcon,
  TrashIcon,
  PencilIcon,
  EnvelopeIcon,
  XMarkIcon,
  PlusIcon,
  FolderIcon,
  ArrowPathIcon,
} from "@heroicons/react/24/outline";
import { UserResponseDto, ProjectResponseDto, RoleResponseDto } from "../../types/responseDTOs";
import toast from "react-hot-toast";
// import ConfirmModal from "../../project_management/user/DeleteModal";
import {
  getAllUsers,
  deleteUser,
} from "@/app/lib/client_service/user_services.client";
import EditSysUser from "../../components/SiteManagementPortal/EditSysUser";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { sendEmail } from "@/app/lib/client_service/notification_services.client";
import { getAllProjects } from "@/app/lib/client_service/projects_services.client";
import { getAllRoles } from "@/app/lib/client_service/role_services.client";

interface Props {
  members: UserResponseDto[];
}

// Extended type that includes pending invites
type TableRow = {
  id: number;
  name: string;
  email: string;
  username: string | null;
  isActive: boolean;
  isArchived: boolean;
  isSysAdmin: boolean;
  isPending?: boolean;
  invitedAt?: string;
  projectName?: string;
  roleName?: string;
};

const UsersTable = ({ members }: Props) => {
  const [tableData, setTableData] = useState<TableRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [showInviteModal, setShowInviteModal] = useState(false);
  const [inviteEmail, setInviteEmail] = useState("");
  const [selectedProjectId, setSelectedProjectId] = useState("");
  const [selectedRoleId, setSelectedRoleId] = useState("");
  const [availableProjects, setAvailableProjects] = useState<ProjectResponseDto[]>([]);
  const [availableRoles, setAvailableRoles] = useState<RoleResponseDto[]>([]);
  const [modalLoading, setModalLoading] = useState(false);
  const [editingUserId, setEditingUserId] = useState<number | null>(null);
  const [editUserName, setEditUserName] = useState("");
  const [confirmModal, setConfirmModal] = useState<{
    isOpen: boolean;
    itemId: number | null;
    itemName: string;
    isPending: boolean;
  }>({
    isOpen: false,
    itemId: null,
    itemName: "",
    isPending: false,
  });
  const { organization } = useOrganizationSession();

  const loadAllData = async () => {
    try {
      // Load active users
      const users = await getAllUsers(organization?.organizationId);
      
      // TODO: Load pending invites from backend
      // const pendingInvites = await getPendingInvites(organization?.organizationId);
      
      // Mock pending invites for now
      const mockPendingInvites = [
        {
          id: 9999,
          name: "",
          email: "pending@example.com",
          username: null,
          isActive: false,
          isArchived: false,
          isSysAdmin: false,
          isPending: true,
          invitedAt: new Date().toISOString(),
          projectName: "Alpha Project",
          roleName: "Developer",
        },
      ];

      // Combine users and pending invites
      const activeUsers: TableRow[] = users.map((user) => ({
        id: user.id,
        name: user.name || "",
        email: user.email || "",
        username: user.username,
        isActive: user.isActive,
        isArchived: user.isArchived,
        isSysAdmin: user.isSysAdmin,
        isPending: false,
      }));

      // Combine and sort: pending invites first, then active users
      const combined = [...mockPendingInvites, ...activeUsers];
      setTableData(combined);
    } catch (error) {
      console.error("Failed to load data:", error);
    }
  };

  useEffect(() => {
    loadAllData();
  }, [members, organization?.organizationId]);

  const handleOpenInviteModal = async () => {
    setShowInviteModal(true);
    setModalLoading(true);

    try {
      const projects = await getAllProjects(organization?.organizationId as number);
      setAvailableProjects(projects);
    } catch (error) {
      console.error("Failed to fetch projects:", error);
      toast.error("Unable to load projects");
    } finally {
      setModalLoading(false);
    }
  };

  useEffect(() => {
    const fetchRolesForProject = async () => {
      if (!selectedProjectId || !organization?.organizationId) {
        setAvailableRoles([]);
        return;
      }

      try {
        const roles = await getAllRoles(
          organization.organizationId as number,
          Number(selectedProjectId)
        );
        setAvailableRoles(roles);
      } catch (error) {
        console.error("Failed to fetch roles:", error);
        toast.error("Unable to load roles for selected project");
      }
    };

    fetchRolesForProject();
  }, [selectedProjectId, organization?.organizationId]);

  const handleInviteUser = async () => {
    if (!inviteEmail) {
      toast.error("Please enter an email address");
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(inviteEmail)) {
      toast.error("Please enter a valid email address");
      return;
    }

    if (selectedProjectId && !selectedRoleId) {
      toast.error("Please select a role for the project");
      return;
    }

    try {
      setModalLoading(true);
      
      await sendEmail(inviteEmail, "New User");
      
      // TODO: Store pending assignment when backend is ready
      // if (selectedProjectId && selectedRoleId) {
      //   await createPendingAssignment(inviteEmail, projectId, roleId);
      // }
      
      if (selectedProjectId && selectedRoleId) {
        toast.success(
          `Invitation sent to ${inviteEmail}. They will be added to the selected project upon accepting.`
        );
      } else {
        toast.success(`Invitation sent to ${inviteEmail}`);
      }
      
      await loadAllData();
      
      setShowInviteModal(false);
      setInviteEmail("");
      setSelectedProjectId("");
      setSelectedRoleId("");
    } catch (error) {
      console.error("Error inviting user:", error);
      toast.error("Failed to send invitation");
    } finally {
      setModalLoading(false);
    }
  };

  const handleResendInvite = async (email: string) => {
    try {
      setLoading(true);
      await sendEmail(email, "Resend Invitation");
      toast.success(`Invitation resent to ${email}`);
    } catch (error) {
      console.error("Failed to resend invite:", error);
      toast.error("Failed to resend invitation");
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveOrCancel = async () => {
    if (!confirmModal.itemId) return;

    try {
      setLoading(true);
      
      if (confirmModal.isPending) {
        // TODO: API call to cancel invite
        // await cancelPendingInvite(confirmModal.itemId);
        toast.success("Invitation cancelled");
      } else {
        await deleteUser(confirmModal.itemId);
        toast.success("User removed from organization");
      }
      
      await loadAllData();
      
      setConfirmModal({
        isOpen: false,
        itemId: null,
        itemName: "",
        isPending: false,
      });
    } catch (error) {
      console.error("Failed to remove/cancel:", error);
      toast.error(confirmModal.isPending ? "Failed to cancel invitation" : "Failed to remove user");
    } finally {
      setLoading(false);
    }
  };

  const activeUserCount = tableData.filter((u) => !u.isPending && u.isActive && !u.isArchived).length;
  const pendingCount = tableData.filter((u) => u.isPending).length;

  return (
    <div className="p-6">
      <div className="card bg-base-100 border border-primary">
        <div className="card-body">
          {/* Header */}
          <div className="flex justify-between items-center mb-6">
            <div>
              <h2 className="text-2xl font-bold">Organization Users</h2>
              <p className="text-base-content/70 text-sm mt-1">
                Manage users in your organization. Invite new users via email or add them directly.
              </p>
            </div>
            <button
              className="btn btn-primary gap-2"
              onClick={handleOpenInviteModal}
              disabled={loading}
            >
              <PlusIcon className="w-5 h-5" />
              Invite User
            </button>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div className="stat bg-base-200 rounded-lg">
              <div className="stat-figure text-primary">
                <UserIcon className="w-8 h-8" />
              </div>
              <div className="stat-title">Active Users</div>
              <div className="stat-value text-primary">{activeUserCount}</div>
            </div>
            <div className="stat bg-base-200 rounded-lg">
              <div className="stat-figure text-warning">
                <EnvelopeIcon className="w-8 h-8" />
              </div>
              <div className="stat-title">Pending Invites</div>
              <div className="stat-value text-warning">{pendingCount}</div>
            </div>
            <div className="stat bg-base-200 rounded-lg">
              <div className="stat-figure text-secondary">
                <UserGroupIcon className="w-8 h-8" />
              </div>
              <div className="stat-title">Total</div>
              <div className="stat-value text-secondary">
                {activeUserCount + pendingCount}
              </div>
            </div>
          </div>

          {/* Combined Users & Pending Invites Table */}
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Email</th>
                  <th>Username</th>
                  <th>Status</th>
                  <th>Project Assignment</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {tableData.length === 0 ? (
                  <tr>
                    <td
                      colSpan={6}
                      className="text-center py-8 text-base-content/70"
                    >
                      No users or pending invites. Click "Invite User" to get started.
                    </td>
                  </tr>
                ) : (
                  tableData.map((row) => (
                    <tr 
                      key={`${row.isPending ? 'pending' : 'user'}-${row.id}`} 
                      className={row.isPending ? "bg-warning/10" : "hover"}
                    >
                      {/* User Column */}
                      <td>
                        <div className="flex items-center gap-2">
                          {row.isPending ? (
                            <>
                              <div className="avatar placeholder">
                                <div className="bg-warning text-warning-content rounded-full w-8">
                                  <EnvelopeIcon className="w-4 h-4" />
                                </div>
                              </div>
                              <div className="font-medium text-base-content/70">
                                Pending Invite
                              </div>
                            </>
                          ) : (
                            <>
                              <div className="font-medium">{row.name}</div>
                              {row.isSysAdmin && (
                                <div className="badge badge-warning badge-sm">Admin</div>
                              )}
                            </>
                          )}
                        </div>
                      </td>

                      {/* Email Column */}
                      <td className="text-base-content/70">{row.email}</td>

                      {/* Username Column */}
                      <td className="text-base-content/70">
                        {row.isPending ? "—" : (row.username || "—")}
                      </td>

                      {/* Status Column */}
                      <td>
                        {row.isPending ? (
                          <div className="badge badge-warning gap-1">
                            <EnvelopeIcon className="w-3 h-3" />
                            Pending
                          </div>
                        ) : row.isArchived ? (
                          <div className="badge badge-error">Archived</div>
                        ) : row.isActive ? (
                          <div className="badge badge-success">Active</div>
                        ) : (
                          <div className="badge badge-warning">Inactive</div>
                        )}
                      </td>

                      {/* Project Assignment Column */}
                      <td>
                        {row.projectName ? (
                          <div className="flex items-center gap-2 text-sm">
                            <FolderIcon className="w-4 h-4 text-base-content/50" />
                            <span>{row.projectName}</span>
                            {row.roleName && (
                              <span className="badge badge-sm badge-outline">
                                {row.roleName}
                              </span>
                            )}
                          </div>
                        ) : (
                          <span className="text-base-content/50 text-sm">—</span>
                        )}
                      </td>

                      {/* Actions Column */}
                      <td>
                        <div className="flex gap-2">
                          {row.isPending ? (
                            <>
                              <button
                                className="btn btn-ghost btn-sm gap-1"
                                onClick={() => handleResendInvite(row.email)}
                                disabled={loading}
                                title="Resend invitation"
                              >
                                <ArrowPathIcon className="w-4 h-4" />
                              </button>
                              <button
                                className="btn btn-ghost btn-sm text-error"
                                onClick={() =>
                                  setConfirmModal({
                                    isOpen: true,
                                    itemId: row.id,
                                    itemName: row.email,
                                    isPending: true,
                                  })
                                }
                                disabled={loading}
                                title="Cancel invitation"
                              >
                                <XMarkIcon className="w-4 h-4" />
                              </button>
                            </>
                          ) : (
                            <>
                              <button
                                className="btn btn-ghost btn-sm"
                                title="Edit user"
                                onClick={() => {
                                  setEditingUserId(row.id);
                                  setEditUserName(row.name);
                                }}
                                disabled={loading}
                              >
                                <PencilIcon className="w-4 h-4" />
                              </button>
                              <button
                                className="btn btn-ghost btn-sm text-error"
                                title="Remove from organization"
                                onClick={() =>
                                  setConfirmModal({
                                    isOpen: true,
                                    itemId: row.id,
                                    itemName: row.name,
                                    isPending: false,
                                  })
                                }
                                disabled={loading || row.isSysAdmin}
                              >
                                <TrashIcon className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Invite User Modal */}
      {showInviteModal && (
        <div className="modal modal-open">
          <div className="modal-box max-w-2xl">
            <div className="flex justify-between items-center mb-6">
              <h3 className="font-bold text-2xl">Invite User to Organization</h3>
              <button
                className="btn btn-sm btn-circle btn-ghost"
                onClick={() => {
                  setShowInviteModal(false);
                  setInviteEmail("");
                  setSelectedProjectId("");
                  setSelectedRoleId("");
                }}
              >
                <XMarkIcon className="w-5 h-5" />
              </button>
            </div>

            {modalLoading ? (
              <div className="flex justify-center items-center py-12">
                <span className="loading loading-spinner loading-lg"></span>
              </div>
            ) : (
              <>
                <div className="space-y-4">
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
                      onChange={(e) => setInviteEmail(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" && inviteEmail) {
                          handleInviteUser();
                        }
                      }}
                    />
                  </div>

                  <div className="divider">Optional Project Assignment</div>

                  <div className="form-control">
                    <label className="label">
                      <span className="label-text font-semibold mr-2">
                        Assign to Project (Optional)
                      </span>
                    </label>
                    <select
                      className="select select-bordered select-lg"
                      value={selectedProjectId}
                      onChange={(e) => {
                        setSelectedProjectId(e.target.value);
                        setSelectedRoleId("");
                      }}
                    >
                      <option value="">No project (org access only)</option>
                      {availableProjects.map((project) => (
                        <option key={project.id} value={project.id}>
                          {project.name}
                        </option>
                      ))}
                    </select>
                  </div>

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
                        onChange={(e) => setSelectedRoleId(e.target.value)}
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

                  <div className="alert alert-info">
                    <EnvelopeIcon className="w-6 h-6" />
                    <div>
                      <h4 className="font-semibold">Email Notification</h4>
                      <p className="text-sm">
                        An invitation email will be sent with instructions to join the organization.
                        {selectedProjectId && " The user will be automatically assigned to the selected project with the specified role upon accepting."}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="modal-action">
                  <button
                    className="btn btn-ghost"
                    onClick={() => {
                      setShowInviteModal(false);
                      setInviteEmail("");
                      setSelectedProjectId("");
                      setSelectedRoleId("");
                    }}
                    disabled={modalLoading}
                  >
                    Cancel
                  </button>
                  <button
                    className={`btn btn-primary gap-2 ${
                      !inviteEmail || (selectedProjectId && !selectedRoleId) || modalLoading
                        ? "btn-disabled"
                        : ""
                    }`}
                    disabled={!inviteEmail || (selectedProjectId && !selectedRoleId) || modalLoading}
                    onClick={handleInviteUser}
                  >
                    {modalLoading ? (
                      <span className="loading loading-spinner loading-sm"></span>
                    ) : (
                      <EnvelopeIcon className="w-5 h-5" />
                    )}
                    Send Invitation
                  </button>
                </div>
              </>
            )}
          </div>
          <div
            className="modal-backdrop"
            onClick={() => setShowInviteModal(false)}
          ></div>
        </div>
      )}

      {/* Edit User Modal */}
      {editingUserId !== null && (
        <EditSysUser
          isOpen={true}
          onClose={() => {
            setEditingUserId(null);
            setEditUserName("");
          }}
          userId={editingUserId}
          userName={editUserName}
          onUserUpdated={loadAllData}
        />
      )}

      {/* Confirm Delete/Cancel Modal */}
      {/* <ConfirmModal
        isOpen={confirmModal.isOpen}
        onClose={() =>
          setConfirmModal({
            isOpen: false,
            itemId: null,
            itemName: "",
            isPending: false,
          })
        }
        onConfirm={handleRemoveOrCancel}
        title={confirmModal.isPending ? "Cancel Invitation?" : "Remove User?"}
        message={
          confirmModal.isPending
            ? `Are you sure you want to cancel the invitation for ${confirmModal.itemName}? They will not be able to join with this invite link.`
            : `Are you sure you want to remove ${confirmModal.itemName} from this organization? They will lose access to all projects.`
        }
        confirmText={confirmModal.isPending ? "Cancel Invite" : "Remove"}
        cancelText={confirmModal.isPending ? "Keep Invite" : "Cancel"}
        isDestructive={true}
        loading={loading}
      /> */}
    </div>
  );
};

export default UsersTable;