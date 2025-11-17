import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { getAllUsers } from "@/app/lib/user_services.client";
import { addMember } from "@/app/lib/projects_services.client";
import { getAllRoles } from "@/app/lib/role_services.client";
import {
  RoleResponseDto,
  UserResponseDto,
} from "@/app/(home)/types/responseDTOs";

interface AddMemberModalProps {
  isOpen: boolean;
  onClose: () => void;
  projectId: number;
  onMemberAdded: () => void;
}

const AddProjectMember = ({
  isOpen,
  onClose,
  projectId,
  onMemberAdded,
}: AddMemberModalProps) => {
  const { t } = useLanguage();
  const [users, setUsers] = useState<UserResponseDto[]>([]);
  const [roles, setRoles] = useState<RoleResponseDto[]>([]);
  const [selectedUser, setSelectedUser] = useState<number | null>(null);
  const [selectedRole, setSelectedRole] = useState<number | null>(null);

  useEffect(() => {
    if (isOpen) {
      // Fetch users
      getAllUsers()
        .then((response: UserResponseDto[]) => {
          setUsers(response);
        })
        .catch((error) => {
          console.error("Error fetching users:", error);
        });

      // Fetch roles for the specific project
      getAllRoles({ projectId })
        .then((response: RoleResponseDto[]) => {
          setRoles(response);
        })
        .catch((error) => {
          console.error("Error fetching roles:", error);
        });
    }
  }, [isOpen, projectId]);

  const handleUserChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const userId = parseInt(event.target.value, 10);
    setSelectedUser(isNaN(userId) ? null : userId);
  };

  const handleRoleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const roleId = parseInt(event.target.value, 10);
    setSelectedRole(isNaN(roleId) ? null : roleId);
  };

  const handleSave = async () => {
    if (selectedUser) {
      const user = users.find((u) => u.id === selectedUser);
      if (user) {
        try {
          await addMember(projectId, selectedUser, selectedRole || undefined);
          onMemberAdded();
          onClose();
        } catch (error) {
          console.error("Error adding member:", error);
        }
      }
    }
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.ADD_NEW_MEMBER}
            </h3>
            <form method="dialog" className="flex flex-col gap-4">
              <select
                value={selectedUser || ""}
                onChange={handleUserChange}
                className="w-full select select-primary text-neutral"
              >
                <option value="" disabled>
                  {t.translations.SELECT_A_MEMBER}
                </option>
                {users.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.email}
                  </option>
                ))}
              </select>
              <select
                value={selectedRole || ""}
                onChange={handleRoleChange}
                className="w-full select select-primary text-neutral"
              >
                <option value="" disabled>
                  {t.translations.SELECT_A_ROLE} (optional)
                </option>
                {roles.map((role) => (
                  <option key={role.id} value={role.id}>
                    {role.name}
                  </option>
                ))}
              </select>
            </form>
            <div className="modal-action">
              <button className="btn" onClick={onClose}>
                {t.translations.CANCEL}
              </button>
              <button className="btn btn-primary" onClick={handleSave}>
                {t.translations.SAVE}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default AddProjectMember;
