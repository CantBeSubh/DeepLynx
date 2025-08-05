import { createProject } from "@/app/lib/projects_services";
import { useState } from "react";

interface CreateProjectsModalProps {
  isOpen: boolean; // Indicates whether the modal is open
  onClose: () => void; // Function to call when closing the modal
  onProjectCreated: () => void;
}

// Main CreateProject component
const CreateProject = ({
  isOpen,
  onClose,
  onProjectCreated,
}: CreateProjectsModalProps) => {
  const [name, setName] = useState("");
  const [abbreviation, setAbbreviation] = useState("");
  const [description, setDescription] = useState("");
  // TODO: Use the react hot toast ... it uses a lot less code
  const [toastMessage, setToastMessage] = useState("");
  const [toastType, setToastType] = useState<
    "success" | "error" | "info" | null
  >(null);

  const handleSubmit = async () => {
    try {
      await createProject({
        name,
        abbreviation: abbreviation || null,
        description: description || null,
      });

      setToastType("success");
      setToastMessage("Porject Created Successfully");

      setName("");
      setAbbreviation("");
      setDescription("");

      setTimeout(() => {
        onProjectCreated();
        setToastMessage("");
        setToastType(null);
        onClose();
      }, 1000);
    } catch (error) {
      console.error("Failed to create project", error);
      setToastType("error");
      setToastMessage("Failed to create Project");

      setTimeout(() => {
        setToastMessage("");
        setToastType(null);
      }, 2000);
    }
  };

  return (
    <>
      {/* Toast Message */}
      {toastMessage && toastType && (
        <div className="toast toast-top toast-end z-50">
          <div className={`alert alert-${toastType}`}>
            <span>{toastMessage}</span>
          </div>
        </div>
      )}
      {/* Render the modal dialog if isOpen is true */}
      {isOpen && (
        <dialog className="modal modal-open">
          {" "}
          {/* Modal dialog with styles */}
          <div className="modal-box max-w-lg">
            {" "}
            {/* Box for modal content with max width */}
            <h3 className="font-bold text-lg mb-4 text-neutral">
              Create New Project {/* Header for the modal */}
            </h3>
            {/* Form for creating a new project */}
            <form
              method="dialog"
              className="flex flex-col gap-4"
              onSubmit={(e) => {
                e.preventDefault();
                handleSubmit();
              }}
            >
              <input
                type="text"
                placeholder="Name" // Placeholder for project name input
                className="input input-primary w-full" // Input styling
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />
              <input
                type="text"
                placeholder="Abbreviation (optional)"
                className="input input-primary w-full"
                value={abbreviation}
                onChange={(e) => setAbbreviation(e.target.value)}
              />
              <textarea
                placeholder="Description (optional)" // Placeholder for project description
                className="textarea textarea-primary w-full" // Textarea styling
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
              <div className="bg-base-200 p-4 rounded-xl">
                {" "}
                {/* Container for file upload */}
                <label className="form-control">
                  <span className="label-text text-neutral">
                    Upload .owl file (optional) {/* Label for file upload */}
                  </span>
                  <input
                    type="file" // File input for uploading .owl files
                    className="file-input file-input-primary text-neutral w-full" // File input styling
                  />
                </label>
              </div>

              {/* Help text with a link to the Wiki */}
              <p className="cursor-pointer text-xs text-neutral">
                Need Help? Details can be creating or updating a project via an
                ontology file can be found on our <a className="link">Wiki.</a>
              </p>
              {/* Modal Action Buttons */}
              <div className="modal-action">
                <button className="btn" onClick={onClose}>
                  {" "}
                  {/* Cancel button calls onClose */}
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Create
                </button>{" "}
                {/* Save button for saving the project */}
              </div>
            </form>
          </div>
        </dialog>
      )}
    </>
  );
};

export default CreateProject;
