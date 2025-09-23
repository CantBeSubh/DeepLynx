import { useLanguage } from "@/app/contexts/Language";

interface RoleSwapProps {
  isOpen: boolean;
  onClose: () => void;
}

// Main CreateWidget component
const RoleSwap = ({ isOpen, onClose }: RoleSwapProps) => {
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
                <input
                type="text"
                placeholder="Select Role"
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

export default RoleSwap;
