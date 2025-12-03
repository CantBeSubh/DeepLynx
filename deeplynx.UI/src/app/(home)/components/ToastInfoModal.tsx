// src/app/(home)/components/ToastInfoModal.tsx
"use client";
import toast from "react-hot-toast";

type Props = {
  title: string;
  toastId:string;
  infoDisplay:string[];
  onClose?: () => void;
};

const ToastInfoModal: React.FC<Props> = ({
  title,
  toastId,
  infoDisplay,
  onClose,
}) => {

const handleClose = () => {
    toast.dismiss(toastId);
    if(onClose)onClose();
  };

  return (
        <div className="relative">
            <div className="space-y-1 pb-5">
            <div className="flex justify-center w-full">
                <span>{title}</span>
            </div>
            {infoDisplay.length > 0 && infoDisplay.map((display)=>(
                <code key={display} className="block">{display}</code>
            ))}
            </div>
            <div className="flex justify-center w-full">
            <button className="btn btn-primary btn-outline btn-xs" onClick={handleClose}>
            Dismiss
            </button>
            </div>

        </div>
    )
}
export default ToastInfoModal;
