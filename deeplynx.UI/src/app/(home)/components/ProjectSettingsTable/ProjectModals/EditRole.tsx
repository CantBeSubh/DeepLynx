import { useLanguage } from "@/app/contexts/Language";

interface EditRoleProps {
  isOpen: boolean;
  onClose: () => void;
}

// Edit Role Component
const EditRole = ({ isOpen, onClose }:EditRoleProps) => {
  const { t } = useLanguage();
  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.SELECT_A_ROLE}
            </h3>
            {/* Form for adding a new member and selecting their role*/}
            <form method="dialog" className="flex flex-col gap-4">
                <select defaultValue="Select a role" className="w-full select select-primary text-neutral">
                    <option disabled={true} className="w-full select select-bordered">
                        {t.translations.SELECT_A_ROLE}
                    </option>
                    <option className="text-neutral option-primary">
                        {t.translations.ADMIN}
                    </option>
                    <option className="text-neutral option-primary">
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
};

export default EditRole;
