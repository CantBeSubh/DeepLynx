interface CreateProjectsWidgetProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function CreateProject({
  isOpen,
  onClose,
}: CreateProjectsWidgetProps) {
  return (
    <>
      {isOpen && (
        <dialog className=" modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              Create New Project
            </h3>

            {/* Form */}
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Name"
                className="input input-primary w-full"
              />
              <textarea
                placeholder="Description"
                className="textarea textarea-primary w-full"
              />
              <div className="bg-base-200 p-4 rounded-xl">
                <label className="form-control">
                  <span className="label-text text-neutral">
                    Upload .owl file (optional)
                  </span>
                  <input
                    type="file"
                    className="file-input file-input-primary text-neutral w-full"
                  />
                </label>
              </div>

              <p className="cursor-pointer text-xs text-neutral">
                Need Help? Details can be creating or updating a project via an
                ontology file can be found on our <a className="link">Wiki.</a>
              </p>
            </form>

            {/* Modal Action Buttons */}
            <div className="modal-action">
              <button className="btn" onClick={onClose}>
                Cancel
              </button>
              <button className="btn btn-primary">Save</button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
}
