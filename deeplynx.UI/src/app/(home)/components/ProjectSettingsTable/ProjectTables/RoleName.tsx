import React from 'react';
import { useLanguage } from "@/app/contexts/Language";

interface SettingsTabProps {
  toPermissionsTab: () => void;
  onCancel: () => void;
}

const SettingsTab: React.FC<SettingsTabProps> = ({ toPermissionsTab, onCancel }) => {
  const { t } = useLanguage();

  return (
    <div>
      <form method="dialog" className="flex flex-col gap-4 w-full mt-6">
        <input
          type="text"
          placeholder="Role name"
          className="input input-primary w-full"
        />
        <input
          type="text"
          placeholder="Description"
          className="input input-primary w-full"
        />
      </form>
      <div className="modal-action">
        <button className="btn" onClick={onCancel}>
          {t.translations.CANCEL}
        </button>
        <button className="btn btn-primary" onClick={toPermissionsTab}>
          {t.translations.NEXT}
        </button>
      </div>
    </div>
  );
};

export default SettingsTab;