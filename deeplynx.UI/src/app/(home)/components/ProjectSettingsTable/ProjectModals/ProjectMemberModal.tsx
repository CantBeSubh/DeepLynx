import React, { useState, useEffect } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import { getAllUsers } from "@/app/lib/user_services.client";

interface AddMemberModalProps {
  isOpen: boolean;
  onClose: () => void;
}

interface User {
  id: number;
  name: string;
}

// Main AddProjectMember component
const AddProjectMember = ({ isOpen, onClose }: AddMemberModalProps) => {
  const { t } = useLanguage();
  const [users, setUsers] = useState<User[]>([]);
  const [selectedUser, setSelectedUser] = useState<number | null>(null);

  useEffect(() => {
    if (isOpen) {
      getAllUsers()
        .then((response: User[]) => {
          setUsers(response);
        })
        .catch(error => {
          console.error('Error fetching users:', error);
        });
    }
  }, [isOpen]);

  const handleUserChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const userId = parseInt(event.target.value, 10);
    setSelectedUser(isNaN(userId) ? null : userId);
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.ADD_NEW_MEMBER}
            </h3>
            {/* Form for adding a new member and selecting their role */}
            <form method="dialog" className="flex flex-col gap-4">
              <select
                value={selectedUser || ''}
                onChange={handleUserChange}
                className="w-full select select-primary text-neutral"
              >
                <option value="" disabled>
                  {t.translations.SELECT_A_MEMBER}
                </option>
                {users.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.name}
                  </option>
                ))}
              </select>
              <select
                className="w-full select select-primary text-neutral"
              >
                <option value="" disabled>
                  {t.translations.SELECT_A_ROLE}
                </option>
                <option value="admin" className="text-neutral option-primary">
                  {t.translations.ADMIN}
                </option>
                <option value="user" className="text-neutral option-primary">
                  {t.translations.USER}
                </option>
              </select>
            </form>
            {/* Modal Action Buttons */}
            <div className="modal-action">
              <button className="btn" onClick={onClose}>
                {t.translations.CANCEL}
              </button>
              <button className="btn btn-primary" onClick={onClose}>
                {t.translations.SAVE}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
}

export default AddProjectMember;