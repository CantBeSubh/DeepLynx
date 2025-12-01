"use client";

import { useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { sendEmail } from "@/app/lib/client_service/notification_services.client";
import { getAllUsers } from "@/app/lib/client_service/user_services.client";
import { getAllGroups } from "@/app/lib/client_service/group_services.client";
// You’ll need a client-side addMember function mirroring addMemberServer
// Adjust this import to match your actual client service location/name.
import {
  addMemberToProject,
  removeMemberFromProject,
  updateProjectMemberRole,
} from "@/app/lib/client_service/projects_services.client";
import {
  ProjectMemberResponseDto,
  UserResponseDto,
  GroupResponseDto,
  RoleResponseDto,
  ProjectResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { PencilIcon, TrashIcon } from "@heroicons/react/24/outline";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

interface Props {
  members: ProjectMemberResponseDto[];
  roles: RoleResponseDto[];
  project: ProjectResponseDto | null;
}

type MemberType = "user" | "group";

type ProjectMemberTableRow = {
  memberId: number;
  name: string;
  email: string | null;
  role: string | null;
  roleId: number | null;
  memberType: MemberType;
};

type ConfirmModalState = {
  isOpen: boolean;
  memberId: number | null;
  memberName: string;
  memberType: MemberType | null;
  isPending: boolean; // kept for parity / future invites; currently always false
};

type AddMemberModalState = {
  isOpen: boolean;
  memberType: MemberType;
};

type EditRoleModalState = {
  isOpen: boolean;
  memberId: number | null;
  memberName: string;
  memberType: MemberType | null;
  currentRoleId: number | null;
};

/* -------------------------------------------------------------------------- */
/*                            Helper: build table rows                        */
/* -------------------------------------------------------------------------- */

const buildTableData = (
  members: ProjectMemberResponseDto[]
): ProjectMemberTableRow[] => {
  return members.map((m) => {
    // Heuristic: if there is an email, treat as user; otherwise, group
    const memberType: MemberType = m.email ? "user" : "group";

    return {
      memberId: m.memberId as number,
      name: m.name ?? "",
      email: m.email ?? null,
      role: m.role ?? null,
      roleId: m.roleId ?? null,
      memberType,
    };
  });
};

/* -------------------------------------------------------------------------- */
/*                         ProjectUsersTable Component                        */
/* -------------------------------------------------------------------------- */

const ProjectUsersTable = ({ members, roles, project }: Props) => {
  /* ------------------------------------------------------------------------ */
  /*                               Core State                                */
  /* ------------------------------------------------------------------------ */

  const [tableData, setTableData] = useState<ProjectMemberTableRow[]>(() =>
    buildTableData(members)
  );
  const [loading, setLoading] = useState(false);

  /* ------------------------------------------------------------------------ */
  /*                           Add Member Modal State                         */
  /* ------------------------------------------------------------------------ */

  const [addModal, setAddModal] = useState<AddMemberModalState>({
    isOpen: false,
    memberType: "user",
  });
  const [selectedMemberId, setSelectedMemberId] = useState<string>("");
  const [selectedRoleId, setSelectedRoleId] = useState<string>("");

  const [availableUsers, setAvailableUsers] = useState<UserResponseDto[]>([]);
  const [availableGroups, setAvailableGroups] = useState<GroupResponseDto[]>(
    []
  );
  const [modalLoading, setModalLoading] = useState(false);

  /* ------------------------------------------------------------------------ */
  /*                        Confirm Remove / Future Use                       */
  /* ------------------------------------------------------------------------ */

  const [confirmModal, setConfirmModal] = useState<ConfirmModalState>({
    isOpen: false,
    memberId: null,
    memberName: "",
    memberType: null,
    isPending: false,
  });

  /* ------------------------------------------------------------------------ */
  /*                        Edit role for user modal state                    */
  /* ------------------------------------------------------------------------ */

  const [editRoleModal, setEditRoleModal] = useState<EditRoleModalState>({
    isOpen: false,
    memberId: null,
    memberName: "",
    memberType: null,
    currentRoleId: null,
  });
  const [editRoleSelectedId, setEditRoleSelectedId] = useState<string>("");

  const handleOpenEditRoleModal = (row: ProjectMemberTableRow) => {
    setEditRoleModal({
      isOpen: true,
      memberId: row.memberId,
      memberName: row.name,
      memberType: row.memberType,
      currentRoleId: row.roleId ?? null,
    });
    setEditRoleSelectedId(row.roleId ? String(row.roleId) : "");
  };

  const handleSaveMemberRole = async () => {
    if (!organizationId || !projectId) {
      toast.error("Missing organization or project");
      return;
    }

    if (
      !editRoleModal.memberId ||
      !editRoleModal.memberType ||
      !editRoleSelectedId
    ) {
      toast.error("Please select a role");
      return;
    }

    try {
      setLoading(true);

      const roleId = Number(editRoleSelectedId);
      const memberId = editRoleModal.memberId;

      if (editRoleModal.memberType === "user") {
        await updateProjectMemberRole(
          organizationId,
          projectId,
          roleId,
          memberId,
          undefined
        );
      } else {
        await updateProjectMemberRole(
          organizationId,
          projectId,
          roleId,
          undefined,
          memberId
        );
      }

      const selectedRole = roles.find((r) => r.id === roleId);

      // Optimistically update local table data
      setTableData((prev) =>
        prev.map((row) =>
          row.memberId === memberId
            ? { ...row, role: selectedRole?.name ?? null, roleId }
            : row
        )
      );

      toast.success("Member role updated");
    } catch (error) {
      console.error("Failed to update member role:", error);
      toast.error("Failed to update member role");
    } finally {
      setLoading(false);
      setEditRoleModal({
        isOpen: false,
        memberId: null,
        memberName: "",
        memberType: null,
        currentRoleId: null,
      });
      setEditRoleSelectedId("");
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                          Org / Project Context                           */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();

  const organizationId = organization?.organizationId
    ? Number(organization.organizationId)
    : undefined;
  const projectId = project?.id ? Number(project.id) : undefined;

  /* ------------------------------------------------------------------------ */
  /*                     Sync server-provided members -> table                */
  /* ------------------------------------------------------------------------ */

  useEffect(() => {
    setTableData(buildTableData(members));
  }, [members]);

  /* ------------------------------------------------------------------------ */
  /*                               Derived Stats                              */
  /* ------------------------------------------------------------------------ */

  const totalMembers = tableData.length;
  const userCount = useMemo(
    () => tableData.filter((m) => m.memberType === "user").length,
    [tableData]
  );
  const groupCount = useMemo(
    () => tableData.filter((m) => m.memberType === "group").length,
    [tableData]
  );

  /* ------------------------------------------------------------------------ */
  /*                          Add Member: Open Modal                          */
  /* ------------------------------------------------------------------------ */

  const handleOpenAddMemberModal = async (memberType: MemberType = "user") => {
    if (!organizationId) {
      toast.error("No organization selected");
      return;
    }

    setAddModal({ isOpen: true, memberType });
    setModalLoading(true);

    try {
      // Load candidate users/groups for this organization
      const [users, groups] = await Promise.all([
        getAllUsers(organizationId),
        getAllGroups(organizationId),
      ]);
      setAvailableUsers(users);
      setAvailableGroups(groups);
    } catch (error) {
      console.error("Failed to load options for Add Member:", error);
      toast.error("Unable to load users, groups, or roles");
    } finally {
      setModalLoading(false);
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                        Add Member: Confirm Action                        */
  /* ------------------------------------------------------------------------ */

  const handleAddMember = async () => {
    if (!organizationId || !projectId) {
      toast.error("Missing organization or project");
      return;
    }

    if (!selectedMemberId) {
      toast.error(
        addModal.memberType === "user"
          ? "Please select a user"
          : "Please select a group"
      );
      return;
    }

    if (!selectedRoleId) {
      toast.error("Please select a role for this member");
      return;
    }

    try {
      setModalLoading(true);

      const roleId = Number(selectedRoleId);
      const memberId = Number(selectedMemberId);

      // Call project add-member client service
      if (addModal.memberType === "user") {
        await addMemberToProject(organizationId, projectId, {
          roleId,
          userId: memberId,
        });
      } else {
        await addMemberToProject(organizationId, projectId, {
          roleId,
          groupId: memberId,
        });
      }

      // Send email notification if we can resolve an email
      if (addModal.memberType === "user") {
        const user = availableUsers.find((u) => u.id === memberId);
        if (user?.email) {
          try {
            await sendEmail(
              user.email,
              "You've been added to a project in DeepLynx Nexus"
            );
          } catch (emailError) {
            console.error("Failed to send notification email:", emailError);
            // non-blocking: we still added them to the project
          }
        }
      } else {
        // For groups: we don't have individual emails here.
        // In the future you might expand to fan-out emails to group members.
      }

      toast.success("Member added to project");

      // Optimistic: you can re-fetch server data via a parent refresh
      // For now, update local tableData minimally:
      const selectedRole = roles.find((r) => r.id === roleId);
      const nameSource =
        addModal.memberType === "user"
          ? availableUsers.find((u) => u.id === memberId)?.name ?? ""
          : availableGroups.find((g) => g.id === memberId)?.name ?? "";

      const emailSource =
        addModal.memberType === "user"
          ? availableUsers.find((u) => u.id === memberId)?.email ?? null
          : null;

      setTableData((prev) => [
        ...prev,
        {
          memberId,
          name: nameSource,
          email: emailSource,
          role: selectedRole?.name ?? null,
          roleId,
          memberType: addModal.memberType,
        },
      ]);

      // Reset modal state
      setAddModal((prev) => ({ ...prev, isOpen: false }));
      setSelectedMemberId("");
      setSelectedRoleId("");
    } catch (error) {
      console.error("Failed to add member to project:", error);
      toast.error("Failed to add member");
    } finally {
      setModalLoading(false);
    }
  };

  const handleRemoveMember = async () => {
    if (!organizationId || !projectId) {
      toast.error("Missing organization or project");
      return;
    }

    if (!confirmModal.memberId || !confirmModal.memberType) {
      toast.error("No member selected to remove");
      return;
    }

    try {
      setLoading(true);

      const memberId = confirmModal.memberId;

      if (confirmModal.memberType === "user") {
        await removeMemberFromProject(
          organizationId,
          projectId,
          memberId,
          undefined
        );
      } else {
        await removeMemberFromProject(
          organizationId,
          projectId,
          undefined,
          memberId
        );
      }

      // Optimistically update local table data
      setTableData((prev) => prev.filter((row) => row.memberId !== memberId));

      toast.success("Member removed from project");
    } catch (error) {
      console.error("Failed to remove member from project:", error);
      toast.error("Failed to remove member");
    } finally {
      setLoading(false);
      setConfirmModal({
        isOpen: false,
        memberId: null,
        memberName: "",
        memberType: null,
        isPending: false,
      });
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                          Main Render: Header + Table                     */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      <div className="card bg-base-100 border border-primary">
        <div className="card-body">
          {/* Header & Stats */}
          <div className="flex justify-between items-center mb-6">
            <div>
              <h2 className="text-2xl font-bold">Project Members</h2>
              <p className="text-base-content/70 text-sm mt-1">
                Manage users and groups assigned to this project. A role is
                required for each member.
              </p>
            </div>
            <div className="flex gap-2">
              <button
                className="btn btn-outline btn-sm"
                disabled={loading}
                onClick={() => handleOpenAddMemberModal("group")}
              >
                Add Group
              </button>
              <button
                className="btn btn-primary btn-sm"
                disabled={loading}
                onClick={() => handleOpenAddMemberModal("user")}
              >
                Add User
              </button>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div className="stat bg-base-200 rounded-lg">
              <div className="stat-title">Total Members</div>
              <div className="stat-value">{totalMembers}</div>
            </div>
            <div className="stat bg-base-200 rounded-lg">
              <div className="stat-title">Users</div>
              <div className="stat-value">{userCount}</div>
            </div>
            <div className="stat bg-base-200 rounded-lg">
              <div className="stat-title">Groups</div>
              <div className="stat-value">{groupCount}</div>
            </div>
          </div>

          {/* Members Table */}
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>Member</th>
                  <th>Type</th>
                  <th>Email</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {tableData.length === 0 ? (
                  <tr>
                    <td
                      colSpan={5}
                      className="text-center py-8 text-base-content/70"
                    >
                      No members in this project yet. Use &quot;Add User&quot;
                      or &quot;Add Group&quot; to get started.
                    </td>
                  </tr>
                ) : (
                  tableData.map((row) => (
                    <tr
                      key={`${row.memberType}-${row.memberId}`}
                      className="hover"
                    >
                      <td className="flex gap-2">
                        <div>{row.name || "—"}</div>
                        {row.role === "Admin" && (
                          <div className="badge badge-warning badge-sm">
                            {row.role}
                          </div>
                        )}
                      </td>
                      <td className="capitalize">{row.memberType}</td>
                      <td className="text-base-content/70">
                        {row.email || "—"}
                      </td>
                      <td>
                        <div className="flex gap-2">
                          <button
                            className="btn btn-ghost btn-xs"
                            disabled={loading}
                            onClick={() => handleOpenEditRoleModal(row)}
                          >
                            <PencilIcon className="size-6" />
                          </button>
                          <button
                            className="btn btn-ghost btn-xs text-error"
                            disabled={loading}
                            onClick={() =>
                              setConfirmModal({
                                isOpen: true,
                                memberId: row.memberId,
                                memberName: row.name,
                                memberType: row.memberType,
                                isPending: false,
                              })
                            }
                          >
                            <TrashIcon className="size-6 text-error" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* TODO: Implement a Confirm Delete modal similar to org Users if desired */}
          {confirmModal.isOpen && (
            <dialog className="modal modal-open">
              <div className="modal-box">
                <h3 className="font-bold text-lg">Remove Member?</h3>
                <p className="py-4">
                  Are you sure you want to remove{" "}
                  <span className="font-semibold">
                    {confirmModal.memberName}
                  </span>{" "}
                  from this project? They will lose access to this project.
                </p>
                <div className="modal-action">
                  <button
                    className="btn btn-ghost"
                    onClick={() =>
                      setConfirmModal({
                        isOpen: false,
                        memberId: null,
                        memberName: "",
                        memberType: null,
                        isPending: false,
                      })
                    }
                  >
                    Cancel
                  </button>
                  <button
                    className="btn btn-error"
                    disabled={loading}
                    onClick={handleRemoveMember}
                  >
                    {loading ? "Removing..." : "Remove"}
                  </button>
                </div>
              </div>
            </dialog>
          )}

          {/* Add Member Modal */}
          {addModal.isOpen && (
            <dialog className="modal modal-open">
              <div className="modal-box">
                <h3 className="font-bold text-lg">
                  {addModal.memberType === "user"
                    ? "Add User to Project"
                    : "Add Group to Project"}
                </h3>
                <p className="py-2 text-sm text-base-content/70">
                  Select an existing {addModal.memberType} and assign a role. A
                  role is required to add them to the project.
                </p>

                <div className="form-control mt-4">
                  <label className="label">
                    <span className="label-text capitalize">
                      {addModal.memberType}
                    </span>
                  </label>
                  <select
                    className="select select-bordered w-full"
                    value={selectedMemberId}
                    onChange={(e) => setSelectedMemberId(e.target.value)}
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

                <div className="form-control mt-4">
                  <label className="label">
                    <span className="label-text">Role</span>
                  </label>
                  <select
                    className="select select-bordered w-full"
                    value={selectedRoleId}
                    onChange={(e) => setSelectedRoleId(e.target.value)}
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
                    onClick={() => {
                      setAddModal((prev) => ({ ...prev, isOpen: false }));
                      setSelectedMemberId("");
                      setSelectedRoleId("");
                    }}
                    disabled={modalLoading}
                  >
                    Cancel
                  </button>
                  <button
                    className="btn btn-primary"
                    onClick={handleAddMember}
                    disabled={modalLoading}
                  >
                    {modalLoading ? "Adding..." : "Add to Project"}
                  </button>
                </div>
              </div>
            </dialog>
          )}
        </div>
      </div>
      {/* Edit Role Modal */}
      {editRoleModal.isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box">
            <h3 className="font-bold text-lg">Edit Member Role</h3>
            <p className="py-2 text-sm text-base-content/70">
              Change the role for{" "}
              <span className="font-semibold">{editRoleModal.memberName}</span>{" "}
              in this project.
            </p>

            <div className="form-control mt-4">
              <label className="label">
                <span className="label-text">Role</span>
              </label>
              <select
                className="select select-bordered w-full"
                value={editRoleSelectedId}
                onChange={(e) => setEditRoleSelectedId(e.target.value)}
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
                onClick={() => {
                  setEditRoleModal({
                    isOpen: false,
                    memberId: null,
                    memberName: "",
                    memberType: null,
                    currentRoleId: null,
                  });
                  setEditRoleSelectedId("");
                }}
                disabled={loading}
              >
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={handleSaveMemberRole}
                disabled={loading}
              >
                {loading ? "Saving..." : "Save"}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </div>
  );
};

export default ProjectUsersTable;
