import { useLanguage } from "@/app/contexts/Language";

interface AddMemberModalProps {
  isOpen: boolean;
  onClose: () => void;
}

// Main CreateWidget component
const AddMember = ({ isOpen, onClose }: AddMemberModalProps) => {
  const { t } = useLanguage();
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
            {/* Form for adding a new member */}
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Email"
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
