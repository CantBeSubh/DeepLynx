// src/app/(home)/organization_management/users/UsersTable.tsx
"use client";

import { useEffect, useState } from "react";
import toast from "react-hot-toast";

import EditSysUser from "../SiteManagementPortal/EditSysUser";
import {
  ProjectResponseDto,
  RoleResponseDto,
  UserResponseDto,
} from "../../types/responseDTOs";

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { sendEmail } from "@/app/lib/client_service/notification_services.client";
import { removeUserFromOrganization } from "@/app/lib/client_service/organization_services.client";
import { getAllProjects } from "@/app/lib/client_service/projects_services.client";
import { getAllRoles } from "@/app/lib/client_service/role_services.client";
import { getAllUsers } from "@/app/lib/client_service/user_services.client";
import DeleteModal from "./DeleteModal";
import InviteUserModal from "./InviteUserModal";
import UsersHeaderStats from "./UsersHeaderStats";
import UsersListTable from "./UsersListTable";
import { UsersTableRow } from "../../types/types";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

interface Props {
  initialMembers: UserResponseDto[];
  header: string;
  description: string;
  onUsersChange?: () => Promise<void>; // Add this prop
}

type ConfirmModalState = {
  isOpen: boolean;
  itemId: number | null;
  itemName: string;
  isPending: boolean;
};

/* -------------------------------------------------------------------------- */
/*                            Helper: build table rows                        */
/* -------------------------------------------------------------------------- */

const buildTableData = (users: UserResponseDto[]): UsersTableRow[] => {
  // TODO: Replace mock with real pending invites from backend
  const mockPendingInvites: UsersTableRow[] = [
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

  const activeUsers: UsersTableRow[] = users.map((user) => ({
    id: user.id,
    name: user.name || "",
    email: user.email || "",
    username: user.username,
    isActive: user.isActive,
    isArchived: user.isArchived,
    isSysAdmin: user.isSysAdmin,
    isPending: false,
  }));

  return [...mockPendingInvites, ...activeUsers];
};

/* -------------------------------------------------------------------------- */
/*                           UsersTable Component                             */
/* -------------------------------------------------------------------------- */

const UsersTable = ({ initialMembers, header, description, onUsersChange }: Props) => {
  /* ------------------------------------------------------------------------ */
  /*                               Core State                                */
  /* ------------------------------------------------------------------------ */

  const [tableData, setTableData] = useState<UsersTableRow[]>(() =>
    buildTableData(initialMembers)
  );
  const [loading, setLoading] = useState(false);

  /* ------------------------------------------------------------------------ */
  /*                           Invite Modal State                             */
  /* ------------------------------------------------------------------------ */

  const [showInviteModal, setShowInviteModal] = useState(false);
  const [inviteEmail, setInviteEmail] = useState("");
  const [selectedProjectId, setSelectedProjectId] = useState("");
  const [selectedRoleId, setSelectedRoleId] = useState("");
  const [availableProjects, setAvailableProjects] = useState<
    ProjectResponseDto[]
  >([]);
  const [availableRoles, setAvailableRoles] = useState<RoleResponseDto[]>([]);
  const [modalLoading, setModalLoading] = useState(false);

  /* ------------------------------------------------------------------------ */
  /*                            Edit User Modal State                         */
  /* ------------------------------------------------------------------------ */

  const [editingUserId, setEditingUserId] = useState<number | null>(null);
  const [editUserName, setEditUserName] = useState("");

  /* ------------------------------------------------------------------------ */
  /*                         Confirm Remove/Cancel State                      */
  /* ------------------------------------------------------------------------ */

  const [confirmModal, setConfirmModal] = useState<ConfirmModalState>({
    isOpen: false,
    itemId: null,
    itemName: "",
    isPending: false,
  });

  /* ------------------------------------------------------------------------ */
  /*                           Organization Context                           */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();

  /* ------------------------------------------------------------------------ */
  /*                        Data Loading / Normalization                      */
  /* ------------------------------------------------------------------------ */

  const loadAllData = async () => {
    if (!organization?.organizationId) return; // guard

    try {
      const users: UserResponseDto[] = await getAllUsers(
        organization.organizationId
      );
      setTableData(buildTableData(users));
      if (onUsersChange) {
        await onUsersChange();
      }
    } catch (error) {
      console.error("Failed to load data:", error);
    }
  };

  // ✅ When server-side members prop changes, sync local state (no extra fetch)
  useEffect(() => {
    setTableData(buildTableData(initialMembers));
  }, [initialMembers]);

  /* ------------------------------------------------------------------------ */
  /*                        Invite Flow: Open & Options                       */
  /* ------------------------------------------------------------------------ */

  const handleOpenInviteModal = async () => {
    setShowInviteModal(true);
    setModalLoading(true);

    try {
      if (!organization?.organizationId) {
        throw new Error("No organization selected");
      }

      const projects = await getAllProjects(
        organization.organizationId as number
      );
      setAvailableProjects(projects);
    } catch (error) {
      console.error("Failed to fetch projects:", error);
      toast.error("Unable to load projects");
    } finally {
      setModalLoading(false);
    }
  };

  // Load roles when project selection changes
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

  /* ------------------------------------------------------------------------ */
  /*                          Invite Flow: Send Email                         */
  /* ------------------------------------------------------------------------ */

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

      if (selectedProjectId && selectedRoleId) {
        toast.success(
          `Invitation sent to ${inviteEmail}. They will be added to the selected project upon accepting.`
        );
      } else {
        toast.success(`Invitation sent to ${inviteEmail}`);
      }

      // ✅ Now re-fetch from API (already org-scoped)
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

  /* ------------------------------------------------------------------------ */
  /*                  Remove User / Cancel Invite Confirmation                */
  /* ------------------------------------------------------------------------ */

  const handleRemoveOrCancel = async () => {
    if (!confirmModal.itemId) return;

    try {
      setLoading(true);

      if (confirmModal.isPending) {
        // TODO: API call to cancel invite
        toast.success("Invitation cancelled");
      } else {
        if (!organization?.organizationId) {
          throw new Error("No organization selected");
        }

        await removeUserFromOrganization(
          organization.organizationId as number,
          confirmModal.itemId
        );

        toast.success("User removed from organization");
      }

      // ✅ Refresh org-scoped list
      await loadAllData();

      setConfirmModal({
        isOpen: false,
        itemId: null,
        itemName: "",
        isPending: false,
      });
    } catch (error) {
      console.error("Failed to remove/cancel:", error);
      toast.error(
        confirmModal.isPending
          ? "Failed to cancel invitation"
          : "Failed to remove user"
      );
    } finally {
      setLoading(false);
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                               Derived Stats                              */
  /* ------------------------------------------------------------------------ */

  const activeUserCount = tableData.filter(
    (u) => !u.isPending && u.isActive && !u.isArchived
  ).length;
  const pendingCount = tableData.filter((u) => u.isPending).length;
  const totalCount = activeUserCount + pendingCount;

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      <div className="card bg-base-100 border border-primary">
        <div className="card-body">
          {/* Page header & stats */}
          <UsersHeaderStats
            activeUserCount={activeUserCount}
            pendingCount={pendingCount}
            totalCount={totalCount}
            loading={loading}
            onInviteClick={handleOpenInviteModal}
            header={header}
            description={description}
          />

          {/* Combined Users & Pending Invites Table */}
          <UsersListTable
            tableData={tableData}
            loading={loading}
            onResendInvite={handleResendInvite}
            onEditUser={(id: number, name: string) => {
              setEditingUserId(id);
              setEditUserName(name);
            }}
            onOpenConfirm={(item: ConfirmModalState) => setConfirmModal(item)}
          />
        </div>
      </div>

      {/* Invite User Modal */}
      <InviteUserModal
        isOpen={showInviteModal}
        inviteEmail={inviteEmail}
        selectedProjectId={selectedProjectId}
        selectedRoleId={selectedRoleId}
        availableProjects={availableProjects}
        availableRoles={availableRoles}
        modalLoading={modalLoading}
        onClose={() => {
          setShowInviteModal(false);
          setInviteEmail("");
          setSelectedProjectId("");
          setSelectedRoleId("");
        }}
        onInvite={handleInviteUser}
        onChangeEmail={setInviteEmail}
        onChangeProject={(value) => {
          setSelectedProjectId(value);
          setSelectedRoleId("");
        }}
        onChangeRole={setSelectedRoleId}
      />

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
      <DeleteModal
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
      />
    </div>
  );
};

export default UsersTable;
