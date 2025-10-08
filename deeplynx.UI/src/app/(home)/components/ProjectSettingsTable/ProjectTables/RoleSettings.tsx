import React, { useState, useEffect } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import RoleDetails from './RoleDetails';
import { useRouter, useSearchParams } from "next/navigation";
import { getAllRoles, updateRole } from "@/app/lib/role_services.client";
import { RoleResponseDto, PermissionResponseDto } from "../../../types/types";

interface RoleSettingsProps {
  id?: string | string[];
}

const RoleSettings = ({ id }: RoleSettingsProps) => {
  const { t } = useLanguage();
  const router = useRouter();
  const searchParams = useSearchParams();

  const [role, setRole] = useState<RoleResponseDto | null>(null);
  const [permissions, setPermissions] = useState<PermissionResponseDto[]>([]);

  // Fetch role and permissions data on component mount
  useEffect(() => {
    const roleId = searchParams.get('roleId');
    if (roleId) {
      getAllRoles()
        .then((data) => {
          setRole(data.find((r: RoleResponseDto) => r.id === Number(roleId)) || null);
          // Add API call to get permissions based on roleId
        })
        .catch((error) => {
          console.error("Error fetching role data:", error);
        });
    }
  }, [searchParams]);

    const onCancel = () => {
      router.push(`/project/${id}/project_settings?tab=Roles`);
    };

    //Function for saving roles and permissions
    const onSave = async () => {
    if (role) {
      try {
        await updateRole(role.id, {
          name: role.name,
          description: role.description,
        });
        const updatedRoles = await getAllRoles();
        setRole(updatedRoles.find((updatedRole: { id: number; }) => updatedRole.id === role.id) || null);
        router.push(`/project/${id}/project_settings?tab=Roles`);
      } catch (error) {
        console.error("Error updating role:", error);
      }
    }
  };

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="w-full">
          {role && (
            <RoleDetails
              role={role}
              setRole={setRole}
              permissions={permissions}
              setPermissions={setPermissions}
              onCancel={onCancel}
              onSave={onSave}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default RoleSettings;