import { useLanguage } from "@/app/contexts/Language";

interface CreateProjectsWidgetProps {
  isOpen: boolean; // Indicates whether the modal is open
  onClose: () => void; // Function to call when closing the modal
}

// Main CreateProject component
const CreateProject = ({ isOpen, onClose }: CreateProjectsWidgetProps) => {
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
              {t.translations.CREATE_PROJECT} {/* Header for the modal */}
            </h3>
            {/* Form for creating a new project */}
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Name" // Placeholder for project name input
                className="input input-primary w-full" // Input styling
              />
              <textarea
                placeholder="Description" // Placeholder for project description
                className="textarea textarea-primary w-full" // Textarea styling
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

              {/* Help text with a link to the Wiki */}
              <p className="cursor-pointer text-xs text-neutral">
                {t.translations.NEED_HELP_UPLOADING}{" "}
                <a className="link">{t.translations.WIKI}</a>
              </p>
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

export default CreateProject;
