import React, { useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { sendEmail } from "@/app/lib/notification_services.client";

interface AddSysUserProps {
  isOpen: boolean;
  onClose: () => void;
}

// Main CreateWidget component
const AddSysUser = ({ isOpen, onClose }: AddSysUserProps) => {
  const { t } = useLanguage();
  const [email, setEmail] = useState("");

  const handleInvite = async () => {
    try {
      await sendEmail(email);
      alert("Invitation sent successfully!");
    } catch (error) {
      console.error("Error sending email:", error);
      alert("An error occurred while sending the email.");
    }

    onClose();
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.ADD_NEW_USER}
            </h3>
            <input
              type="text"
              placeholder="Email"
              className="input input-primary w-full"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <div className="modal-action">
              <button type="button" className="btn" onClick={onClose}>
                {t.translations.CANCEL}
              </button>
              <button type="submit" className="btn btn-primary" onClick={handleInvite}>
                {t.translations.INVITE}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default AddSysUser;
