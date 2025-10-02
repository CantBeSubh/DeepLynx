import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { updateUser } from "@/app/lib/user_services.client";

interface EditSysUserProps {
  isOpen: boolean;
  onClose: () => void;
  userId: number;
  userName: string;
}

// Main EditSysUser component
const EditSysUser = ({ isOpen, onClose, userId, userName }: EditSysUserProps) => {
  const { t } = useLanguage();
  const [name, setName] = useState(userName);

  useEffect(() => {
    if (isOpen) {
      setName(userName);
    }
  }, [isOpen, userName]);

  const handleUpdate = async (e: React.FormEvent) => {
    try {
      await updateUser(userId, name);
      alert("User updated successfully!");
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
            <form className="flex flex-col gap-4" onSubmit={handleUpdate}>
              <input
                type="text"
                placeholder="Name"
                className="input input-primary w-full"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />
              <div className="modal-action">
                <button type="button" className="btn" onClick={onClose}>
                  {t.translations.CANCEL}
                </button>
                <button type="submit" className="btn btn-primary">
                  {t.translations.SAVE}
                </button>
              </div>
            </form>
          </div>
        </dialog>
      )}
    </>
  );
};

export default EditSysUser;