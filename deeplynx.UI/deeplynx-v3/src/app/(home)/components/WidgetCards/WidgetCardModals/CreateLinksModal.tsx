interface CreateLinkModalProps {
  isOpen: boolean; // Indicates whether the modal is open
  onClose: () => void; // Function to call when closing the modal
}

// Main CreateWidget component
const CreateLink = ({ isOpen, onClose }: CreateLinkModalProps) => {
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
              Create New Link {/* Header for the modal */}
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
                Cancel
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

export default CreateLink;