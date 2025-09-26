import React from 'react';
import { useLanguage } from "@/app/contexts/Language";
import ProjectManagementTable from './ProjectManagementTable';

interface PermissionsTabProps {
  onCancel: () => void;
  onSave: () => void;
}

const PermissionsTab: React.FC<PermissionsTabProps> = ({ onCancel, onSave }) => {
  const { t } = useLanguage();

  return (
    <div>
        <div>
            <ProjectManagementTable data={[]} />
            {/* TODO Add data here for project permissions */}
            <ProjectManagementTable data={[]} />
            {/* TODO Add data here for user management */}
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