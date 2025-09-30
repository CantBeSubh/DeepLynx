import { useLanguage } from "@/app/contexts/Language";

interface AddOrgProps {
  isOpen: boolean;
  onClose: () => void;
}

// Main CreateWidget component
const AddOrg = ({ isOpen, onClose }: AddOrgProps) => {
  const { t } = useLanguage();
  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.ADD_A_NEW_ORGANIZATION}
            </h3>
            {/* Form for adding a new member and selecting their role*/}
            <form method="dialog" className="flex flex-col gap-4">
                <input
                type="text"
                placeholder="Organization Name"
                className="input input-primary w-full"
                />
                <input
                type="text"
                placeholder="Description"
                className="input input-primary w-full"
                />
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
};

export default AddOrg;
