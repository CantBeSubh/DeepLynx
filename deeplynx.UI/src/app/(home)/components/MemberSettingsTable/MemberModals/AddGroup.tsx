// import { useLanguage } from "@/app/contexts/Language";

// interface AddGroupProps {
//   isOpen: boolean;
//   onClose: () => void;
//   onAddGroup: (groupName: string, description: string) => void;
// }

// const AddGroup = ({ isOpen, onClose }: AddGroupProps) => {
//   const { t } = useLanguage();
//   return (
//     <>
//       {isOpen && (
//         <dialog className="modal modal-open">
//           <div className="modal-box max-w-lg">
//             <h3 className="font-bold text-lg mb-4 text-neutral">
//               {t.translations.ADD_A_NEW_GROUP}
//             </h3>
//             {/* Form for adding a new member and selecting their role*/}
//             <form method="dialog" className="flex flex-col gap-4">
//                 <input
//                 type="text"
//                 placeholder="Group Name"
//                 className="input input-primary w-full"
//                 />
//                 <input
//                 type="text"
//                 placeholder="Description"
//                 className="input input-primary w-full"
//                 />
//             </form>
//             {/* Modal Action Buttons */}
//             <div className="modal-action">
//                 <button className="btn" onClick={onClose}>
//                     {t.translations.CANCEL}
//                 </button>
//                 <button className="btn btn-primary" onClick={onClose}>
//                     {t.translations.SAVE}
//                 </button>
//             </div>
//           </div>
//         </dialog>
//       )}
//     </>
//   );
// };

// export default AddGroup;

import { useLanguage } from "@/app/contexts/Language";
import { useState } from "react";

interface AddGroupProps {
  isOpen: boolean;
  onClose: () => void;
  onAddGroup: (groupName: string, description?: string) => void;
}

const AddGroup = ({ isOpen, onClose, onAddGroup }: AddGroupProps) => {
  const { t } = useLanguage();
  const [groupName, setGroupName] = useState("");
  const [description, setDescription] = useState("");

  const handleSave = () => {
    onAddGroup(groupName, description);
    setGroupName("");
    setDescription("");
    onClose();
  };

  return (
    <>
      {isOpen && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-lg">
            <h3 className="font-bold text-lg mb-4 text-neutral">
              {t.translations.ADD_A_NEW_GROUP}
            </h3>
            <form method="dialog" className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Group Name"
                className="input input-primary w-full"
                value={groupName}
                onChange={(e) => setGroupName(e.target.value)}
              />
              <input
                type="text"
                placeholder="Description"
                className="input input-primary w-full"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </form>
            <div className="modal-action">
              <button className="btn" onClick={onClose}>
                {t.translations.CANCEL}
              </button>
              <button className="btn btn-primary" onClick={handleSave}>
                {t.translations.SAVE}
              </button>
            </div>
          </div>
        </dialog>
      )}
    </>
  );
};

export default AddGroup;