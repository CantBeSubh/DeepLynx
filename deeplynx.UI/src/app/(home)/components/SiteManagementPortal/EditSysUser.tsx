import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { updateUser } from "@/app/lib/client_service/user_services.client";
import { setOrganizationAdminStatus } from "@/app/lib/organization_services.client";

interface EditSysUserProps {
  isOpen: boolean;
  onClose: () => void;
  userId: number;
  userName: string;
  // currentAdminStatus: boolean; //need to add backend
  onUserUpdated: () => void;
}

// Main EditSysUser component
const EditSysUser = ({
  isOpen,
  onClose,
  userId,
  userName,
  onUserUpdated,
}: EditSysUserProps) => {
  const { t } = useLanguage();
  const [name, setName] = useState(userName);
  // const [isAdmin, setIsAdmin] = useState(currentAdminStatus);

  useEffect(() => {
    if (isOpen) {
      setName(userName);
      // setIsAdmin(currentAdminStatus);
    }
  }, [isOpen, userName]);

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await updateUser(userId, name);
      // await setOrganizationAdminStatus(organizationId, userId, isAdmin);
      onUserUpdated();
    } catch (error) {
      console.error("Error updating user:", error);
      alert("An error occurred while updating the user.");
    }

    onClose();
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.EDIT_USER}
            </h3>
            <div className="flex flex-col gap-2">
              <label className="font-semibold text-sm text-neutral">
                {t.translations.NAME}
              </label>
              <input
                type="text"
                placeholder="Name"
                className="input input-primary w-full"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />
            </div>
            <div className="flex flex-col gap-2">
              {/* <label className="font-semibold text-sm text-neutral">
                  {t.translations.ADMIN}
                </label>
                <select
                  className="select select-primary w-full"
                // value={isAdmin ? "true" : "false"}
                // onChange={(e) => setIsAdmin(e.target.value === "true")}
                >
                  <option value="false">No</option>
                  <option value="true">Yes</option>
                </select> */}
            </div>
            <div className="modal-action">
              <button type="button" className="btn" onClick={onClose}>
                {t.translations.CANCEL}
              </button>
              <button
                type="submit"
                className="btn btn-primary"
                onClick={handleUpdate}
              >
                {t.translations.SAVE}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default EditSysUser;
