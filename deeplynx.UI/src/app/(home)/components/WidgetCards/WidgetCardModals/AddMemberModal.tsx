import { translations } from "@/app/lib/translations";
import React from "react";

interface AddMemberModalProps {
  isOpen: boolean; // Indicates whether the modal is open
  onClose: () => void; // Function to call when closing the modal
}

// Main CreateWidget component
const AddMember = ({ isOpen, onClose }: AddMemberModalProps) => {
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
              {t.translations.ADD_NEW_MEMBER} {/* Header for the modal */}
            </h3>
            {/* Form for creating a new widget */}
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Name"
                className="input input-primary w-full"
              />
              <input
                placeholder="Role"
                className="input input-primary w-full"
              />
              <div className="bg-base-200 p-4 rounded-xl">
                {" "}
                {/* Container for file upload */}
                <label className="form-control">
                  <span className="label-text text-neutral">
                    {t.translations.UPLOAD_PNG_FILE}{" "}
                    {/* Label for file upload */}
                  </span>
                  <input
                    type="file" // File input for uploading .owl files
                    className="file-input file-input-primary text-neutral w-full" // File input styling
                  />
                </label>
              </div>
            </form>
            {/* Modal Action Buttons */}
            <div className="modal-action">
              <button className="btn" onClick={onClose}>
                {" "}
                {/* Cancel button calls onClose */}
                {t.translations.CANCEL}
              </button>
              <button className="btn btn-primary">Save</button>{" "}
              {/* Save button for saving the project */}
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default AddMember;
