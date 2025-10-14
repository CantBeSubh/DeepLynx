import React from 'react';
import { useLanguage } from "@/app/contexts/Language";
import { RoleResponseDto, PermissionResponseDto } from "../../../types/types";
import RoleManagementTable from './ProjectManagementTable';
interface RoleDetailsProps {
  role: RoleResponseDto;
  setRole: (role: RoleResponseDto) => void;
  permissions: PermissionResponseDto[];
  setPermissions: (permissions: PermissionResponseDto[]) => void;
  onCancel: () => void;
  onSave: () => void;
}

const RoleDetails: React.FC<RoleDetailsProps> = ({
  role,
  setRole,
  permissions,
  setPermissions,
  onCancel,
  onSave
}) => {
  const { t } = useLanguage();

  const handleRoleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setRole({ ...role, [name]: value });
  };

  return (
    <div>
      <h2 className="card-title">{t.translations.ROLE_DETAILS}</h2>
      <form method="dialog" className="flex flex-col gap-4 w-full mt-6">
        <input
          type="text"
          name="name"
          placeholder={role.name || 'Role name'}
          className="input input-primary w-full"
          value={role.name || ''}
          onChange={handleRoleChange}
        />
        <input
          type="text"
          name="description"
          placeholder={role.description || 'Description'}
          className="input input-primary w-full"
          value={role.description || ''}
          onChange={handleRoleChange}
        />
        <div className="mt-6">
          <h2 className="card-title">{t.translations.ROLE_PERMISSIONS}</h2>
          <RoleManagementTable
            projectData={permissions.filter(p => p.resource === 'project')}
            //Add back when enabling permissions
            // userData={permissions.filter(p => p.resource === 'user')}
          />
        </div>
        <div className="modal-action">
          <button className="btn" onClick={onCancel}>
            {t.translations.CANCEL}
          </button>
          <button className="btn btn-primary" onClick={onSave}>
            {t.translations.SAVE}
          </button>
        </div>
      </form>
    </div>
  );
};

export default RoleDetails;