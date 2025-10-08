import { useLanguage } from "@/app/contexts/Language";
import { useState, useEffect } from "react";
import { createRole } from "@/app/lib/role_services.client";
import { RoleResponseDto } from '../../../types/types';


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

interface AddRoleProps {
  isOpen: boolean;
  onClose: () => void;
  onAddRole: (groupName: string, description?: string) => void;
}

const AddRole = ({ isOpen, onClose, onAddRole }: AddRoleProps) => {
  const { t } = useLanguage();
  const [groupName, setGroupName] = useState("");
  const [description, setDescription] = useState("");

  const handleSave = () => {
    onAddRole("");
    setGroupName("");
    setDescription("");
    onClose();
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.ADD_A_NEW_ROLE}
            </h3>
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Group Name"
                className="input input-primary w-full"
                value={groupName}
                onChange={(e) => setGroupName(e.target.value)}
              />
              <input
                type="text"
                placeholder="Description"
                className="input input-primary w-full"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
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

export default AddRole;