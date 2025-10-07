import { useLanguage } from "@/app/contexts/Language";
import { useState, useEffect } from "react";
import { getAllRoles } from "@/app/lib/role_services.client";
import { ProjectMembersTable } from "@/app/(home)/types/types";

interface Role {
  id: number;
  name: string;
  description: string | null;
  lastUpdatedAt: string;
  lastUpdatedBy: string | null;
  isArchived: boolean;
  projectId: number;
  organizationId: number | null;
}

interface RoleSwapProps {
  isOpen: boolean;
  onClose: () => void;
  onRoleUpdate: (newRoleId: number, newRoleName: string) => Promise<void>;
  currentMember: ProjectMembersTable | null;
  projectId: string | null;
  selectedMembers?: ProjectMembersTable[];
  roles?: Role[]; // Optional: can be passed from parent or fetched here
}

const RoleSwap = ({
  isOpen,
  onClose,
  onRoleUpdate,
  currentMember,
  projectId,
  selectedMembers,
  roles: rolesFromParent
}: RoleSwapProps) => {
  const { t } = useLanguage();
  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(null);
  const [roles, setRoles] = useState<Role[]>(rolesFromParent || []);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // If roles are passed from parent, use them
    if (rolesFromParent) {
      setRoles(rolesFromParent);
    } else if (isOpen && projectId) {
      // Otherwise, fetch them
      fetchRoles();
    }
  }, [isOpen, projectId, rolesFromParent]);

  useEffect(() => {
    // Reset selected role when modal opens with a new member
    if (currentMember) {
      setSelectedRoleId(currentMember.roleId || null);
    }
  }, [currentMember]);

  const fetchRoles = async () => {
    try {
      const rolesData = await getAllRoles(Number(projectId));
      setRoles(rolesData);
    } catch (error) {
      console.error("Error fetching roles:", error);
    }
  };

  const handleSave = async () => {
    if (!selectedRoleId) {
      alert("Please select a role");
      return;
    }

    // Get the role name for the selected role ID
    const selectedRole = roles.find(role => role.id === selectedRoleId);
    const roleName = selectedRole?.name || 'Unknown';

    setLoading(true);
    try {
      // Pass both roleId and roleName to the parent component
      await onRoleUpdate(selectedRoleId, roleName);
      onClose();
    } catch (error) {
      console.error("Error updating role:", error);
      alert("Failed to update role");
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setSelectedRoleId(null);
    onClose();
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.SELECT_A_ROLE}
            </h3>

            {/* Display member info if single member */}
            {currentMember && !selectedMembers && (
              <div className="mb-4 p-3 bg-base-200 rounded">
                <p className="text-sm">
                  <span className="font-semibold">{t.translations.MEMBER}:</span> {currentMember.name}
                </p>
                {currentMember.email && (
                  <p className="text-sm text-gray-500">{currentMember.email}</p>
                )}
              </div>
            )}

            {/* Display count if multiple members */}
            {selectedMembers && selectedMembers.length > 0 && (
              <div className="mb-4 p-3 bg-base-200 rounded">
                <p className="text-sm">
                  <span className="font-semibold">
                    {"Updating role for"}:
                  </span>{" "}
                  {selectedMembers.length} {t.translations.MEMBERS || "members"}
                </p>
              </div>
            )}

            {/* Role Selection Form */}
            <form onSubmit={(e) => e.preventDefault()} className="flex flex-col gap-4">
              <select
                value={selectedRoleId || ""}
                onChange={(e) => setSelectedRoleId(Number(e.target.value))}
                className="w-full select select-primary text-neutral"
              >
                <option value="" disabled>
                  {t.translations.SELECT_A_ROLE}
                </option>
                {roles.map((role) => (
                  <option key={role.id} value={role.id} className="text-neutral">
                    {role.name}
                  </option>
                ))}
              </select>
            </form>

            {/* Modal Action Buttons */}
            <div className="modal-action">
              <button
                className="btn"
                onClick={handleClose}
                disabled={loading}
              >
                {t.translations.CANCEL}
              </button>
              <button
                className="btn btn-primary"
                onClick={handleSave}
                disabled={loading || !selectedRoleId}
              >
                {loading ? t.translations.SAVING || "Saving..." : t.translations.SAVE}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default RoleSwap;