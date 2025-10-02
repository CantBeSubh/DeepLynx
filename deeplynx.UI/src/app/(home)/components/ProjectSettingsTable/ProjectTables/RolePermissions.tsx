import React from 'react';
import { useLanguage } from "@/app/contexts/Language";
import { projectPermissions, userPermissions } from "../../../dummy_data/data";
import ProjectManagementTable from './ProjectManagementTable';
import UserManagementTable from './UserManagementTable';

interface PermissionsTabProps {
  onCancel: () => void;
  onSave: () => void;
}

const PermissionsTab: React.FC<PermissionsTabProps> = ({ onCancel, onSave }) => {
  const { t } = useLanguage();

  return (
    <div>
        <div>
            <ProjectManagementTable data={projectPermissions} />
            <UserManagementTable data={userPermissions} />
        </div>
      <div className="modal-action">
        <button className="btn" onClick={onCancel}>
          {t.translations.CANCEL}
        </button>
        <button className="btn btn-primary" onClick={onSave}>
          {t.translations.SAVE}
        </button>
      </div>
    </div>
  );
};

export default PermissionsTab;