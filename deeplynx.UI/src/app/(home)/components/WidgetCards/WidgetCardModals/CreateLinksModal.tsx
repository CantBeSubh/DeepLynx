import { translations } from "@/app/lib/translations";
import React from "react";

interface CreateLinkModalProps {
  isOpen: boolean; // Indicates whether the modal is open
  onClose: () => void; // Function to call when closing the modal
}

// Main CreateWidget component
const CreateLink = ({ isOpen, onClose }: CreateLinkModalProps) => {
  const locale = "en";
  const t = translations[locale];
  return (
    <>
      {/* Render the modal dialog if isOpen is true */}
      {isOpen && (
        <dialog className="modal modal-open">
          {" "}
          {/* Modal dialog with styles */}
          <div className="modal-box max-w-lg">
            {" "}
            {/* Box for modal content with max width */}
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.CREATE_NEW_LINK} {/* Header for the modal */}
            </h3>
            {/* Form for creating a new widget */}
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Label"
                className="input input-primary w-full"
              />
              <input
                placeholder="Link"
                className="input input-primary w-full"
              />
            </form>
            {/* Modal Action Buttons */}
            <div className="modal-action">
              <button className="btn" onClick={onClose}>
                {" "}
                {/* Cancel button calls onClose */}
                {t.translations.CANCEL}
              </button>
              <button className="btn btn-primary">{t.translations.SAVE}</button>{" "}
              {/* Save button for saving the project */}
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default CreateLink;
